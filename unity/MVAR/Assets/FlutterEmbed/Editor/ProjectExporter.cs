using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

internal abstract class ProjectExporter {
    internal void Export(BuildPlayerOptions buildPlayerOptions, List<string> precheckWarnings)
    {        
        // Disable engine code stripping to prevent Unity 6 missing internal call linker errors (BurstGetBindingsFunctionsForModule).
        // KEEP stripUnusedMeshComponents = false to avoid the 72 billion URP variant mesh channel evaluation hang.
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.stripUnusedMeshComponents = false;

        if (buildPlayerOptions.target == BuildTarget.Android)
        {
            // Only compile Android OpenGLES3 shaders (disable Vulkan to avoid huge memory & variant allocation)
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            // Use Medium stripping level and optimize IL2CPP for size
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.Android, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Batasi worker IL2CPP ke 4 worker agar aman dan stabil di PC RAM 8GB (mencegah memory thrashing/OOM)
            PlayerSettings.SetAdditionalIl2CppArgs("--jobs=4");
        }

        // Force LZ4HC (High Compression) for Android and iOS export to make the build much lighter and compact.
        buildPlayerOptions.options &= ~BuildOptions.CompressWithLz4;
        buildPlayerOptions.options |= BuildOptions.CompressWithLz4HC;

        // Clear debug/development flags to ensure a clean, lightweight release build
        buildPlayerOptions.options &= ~BuildOptions.Development;
        buildPlayerOptions.options &= ~BuildOptions.AllowDebugging;
        buildPlayerOptions.options &= ~BuildOptions.ConnectWithProfiler;
        // Ensure Mobile_RPAsset is the active render pipeline to prevent generic shader explosion
        var rpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        if (rpAsset != null)
        {
            GraphicsSettings.defaultRenderPipeline = rpAsset;
            QualitySettings.renderPipeline = rpAsset;
        }

        // Strip legacy and unneeded shaders from GraphicsSettings Always Included Shaders list
        CleanAlwaysIncludedShaders();

        Debug.Log("Starting Unity Player Build with LZ4HC compression, GLES3 shader stripping, and IL2CPP size optimization...");

        // This executes the build:
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result != BuildResult.Succeeded) {
            Debug.LogError("Building project for Flutter failed");
            
            if (Application.isBatchMode) {
                // throwing an exception shows an error on the command line, exit(1) doesn't.
                throw new System.Exception("Building project for Flutter failed");
            }
        }
        else {
            TransformExportedProject(buildPlayerOptions.locationPathName);

            // Debug.Log doesn't work until after BuildPipeline.BuildPlayer has executed
            foreach(var log in precheckWarnings) {
                Debug.LogWarning(log);
            }
            Debug.Log($"Building project for Flutter succeeded with LZ4HC compression and optimized shader variants.");

            if (Application.isBatchMode) {
                EditorApplication.Exit(0);
            }
        }
    }

    private static void CleanAlwaysIncludedShaders()
    {
        try
        {
            var graphicsSettingsAsset = GraphicsSettings.GetGraphicsSettings();
            if (graphicsSettingsAsset == null) return;

            var serializedObj = new SerializedObject(graphicsSettingsAsset);
            var alwaysIncludedProp = serializedObj.FindProperty("m_AlwaysIncludedShaders");
            if (alwaysIncludedProp != null && alwaysIncludedProp.isArray)
            {
                bool modified = false;
                for (int i = alwaysIncludedProp.arraySize - 1; i >= 0; i--)
                {
                    var elem = alwaysIncludedProp.GetArrayElementAtIndex(i);
                    var shader = elem.objectReferenceValue as Shader;
                    if (shader != null)
                    {
                        string sName = shader.name;
                        if (sName == "Diffuse" || sName.StartsWith("Legacy") || sName.StartsWith("Standard"))
                        {
                            alwaysIncludedProp.DeleteArrayElementAtIndex(i);
                            modified = true;
                            Debug.Log($"Removed unused shader '{sName}' from Always Included Shaders.");
                        }
                    }
                }
                if (modified)
                {
                    serializedObj.ApplyModifiedProperties();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("CleanAlwaysIncludedShaders notice: " + ex.Message);
        }
    }

    protected abstract void TransformExportedProject(string exportPath);
}