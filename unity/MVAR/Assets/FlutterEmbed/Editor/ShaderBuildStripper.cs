using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Strips out all heavy and unused shader variants and compute shaders,
/// keeping only the bare minimum functionality required for mobile AR.
/// </summary>
public class ShaderBuildStripper : IPreprocessShaders, IPreprocessComputeShaders
{
    public int callbackOrder => 0;

    // Passes that are never needed in forward mobile AR
    private static readonly HashSet<string> StrippedPassNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "GBuffer",
        "DepthNormals",
        "Meta",
        "Universal2D",
        "MotionVectors",
        "ScreenSpaceShadows",
        "ShadowCaster"
    };

    // Shaders to completely eliminate from the build
    private static readonly HashSet<string> ExcludedShaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Standard",
        "Standard (Specular setup)",
        "Standard (Roughness setup)",
        "Diffuse",
        "Autodesk Interactive",
        "Nature/SpeedTree",
        "Nature/Tree Creator Bark",
        "Nature/Tree Creator Leaves",
        "Nature/Tree Soft Occlusion Bark",
        "Nature/Tree Soft Occlusion Leaves"
    };

    private static readonly string[] ExcludedShaderPrefixes = new[]
    {
        "Hidden/Sentis/",
        "Hidden/Nature/",
        "Hidden/TreeEditor/",
        "Hidden/TerrainEngine/",
        "Hidden/Universal Render Pipeline/Debug",
        "Hidden/Universal/Debug",
        "Hidden/Internal-Debug",
        "Legacy Shaders/",
        "Particles/Standard Unlit",
        "Particles/Standard Surface"
    };

    private static readonly ShaderKeyword[] StrippedKeywords = new[]
    {
        // Lightmapping and baked GI
        new ShaderKeyword("LIGHTMAP_ON"),
        new ShaderKeyword("DIRLIGHTMAP_COMBINED"),
        new ShaderKeyword("DYNAMICLIGHTMAP_ON"),
        new ShaderKeyword("LIGHTMAP_SHADOW_MIXING"),
        new ShaderKeyword("SHADOWS_SHADOWMASK"),
        new ShaderKeyword("BAKED_GI_MODIFIERS"),
        new ShaderKeyword("PROBE_VOLUMES_L1"),
        new ShaderKeyword("PROBE_VOLUMES_L2"),

        // Fog
        new ShaderKeyword("FOG_LINEAR"),
        new ShaderKeyword("FOG_EXP"),
        new ShaderKeyword("FOG_EXP2"),

        // Cascades and complex shadows
        new ShaderKeyword("_MAIN_LIGHT_SHADOWS_CASCADE"),
        new ShaderKeyword("_MAIN_LIGHT_SHADOWS_SCREEN"),
        new ShaderKeyword("_ADDITIONAL_LIGHT_SHADOWS"),
        new ShaderKeyword("_SHADOWS_SOFT"),
        new ShaderKeyword("_SHADOWS_SOFT_LOW"),
        new ShaderKeyword("_SHADOWS_SOFT_MEDIUM"),
        new ShaderKeyword("_SHADOWS_SOFT_HIGH"),
        new ShaderKeyword("SHADOWS_SCREEN"),

        // Deferred and screen space effects
        new ShaderKeyword("_DEFERRED_STENCIL"),
        new ShaderKeyword("_DEFERRED_FIRST_LIGHT"),
        new ShaderKeyword("_DEFERRED_MAIN_LIGHT"),
        new ShaderKeyword("_GBUFFER_NORMALS_OCT"),
        new ShaderKeyword("_SCREEN_SPACE_OCCLUSION"),

        // Reflection probe complexity
        new ShaderKeyword("_REFLECTION_PROBE_BLENDING"),
        new ShaderKeyword("_REFLECTION_PROBE_BOX_PROJECTION"),

        // Heavy material features not used in AR
        new ShaderKeyword("_CLEARCOAT"),
        new ShaderKeyword("_PARALLAXMAP"),
        new ShaderKeyword("_DETAIL"),
        new ShaderKeyword("_ALPHAPREMULTIPLY_ON"),

        // Debug & visualization
        new ShaderKeyword("DEBUG_DISPLAY"),
        new ShaderKeyword("EDITOR_VISUALIZATION"),
        new ShaderKeyword("DOTS_INSTANCING_ON")
    };

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shader == null || data == null || data.Count == 0) return;

        string shaderName = shader.name;

        // 1. Strip non-essential passes completely (GBuffer, Meta, MotionVectors, ShadowCaster, etc.)
        if (!string.IsNullOrEmpty(snippet.passName) && StrippedPassNames.Contains(snippet.passName))
        {
            data.Clear();
            return;
        }

        // 2. Strip unused/legacy/debug shaders entirely
        if (ExcludedShaderNames.Contains(shaderName))
        {
            data.Clear();
            return;
        }

        for (int p = 0; p < ExcludedShaderPrefixes.Length; p++)
        {
            if (shaderName.StartsWith(ExcludedShaderPrefixes[p], StringComparison.OrdinalIgnoreCase))
            {
                data.Clear();
                return;
            }
        }

        // 3. Remove non-OpenGLES3 platforms when targeting Android
        bool isAndroid = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);
        if (isAndroid)
        {
            for (int i = data.Count - 1; i >= 0; i--)
            {
                if (data[i].shaderCompilerPlatform != ShaderCompilerPlatform.GLES3x)
                {
                    data.RemoveAt(i);
                }
            }
        }

        if (data.Count == 0) return;

        // 4. Strip variants containing heavy/unused keywords
        for (int i = data.Count - 1; i >= 0; i--)
        {
            var compilerData = data[i];
            bool shouldStrip = false;
            for (int k = 0; k < StrippedKeywords.Length; k++)
            {
                if (compilerData.shaderKeywordSet.IsEnabled(StrippedKeywords[k]))
                {
                    shouldStrip = true;
                    break;
                }
            }

            if (shouldStrip)
            {
                data.RemoveAt(i);
            }
        }

        // 5. Capping URP shader variants:
        // Keep at most 2 variants per pass (basic unlit/directional lit) to make build lightning-fast
        // without sacrificing visual appearance of AR chemical models.
        if (shaderName.StartsWith("Universal Render Pipeline/", StringComparison.OrdinalIgnoreCase))
        {
            while (data.Count > 2)
            {
                data.RemoveAt(data.Count - 1);
            }
        }
    }

    public void OnProcessComputeShader(ComputeShader shader, string kernelName, IList<ShaderCompilerData> data)
    {
        // Dikosongkan semua: AR Vuforia Android GLES3 sama sekali tidak memakai compute shader.
        data.Clear();
    }
}
