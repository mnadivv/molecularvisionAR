using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FlutterEmbedBuildTemp
{
    [MenuItem("Flutter Embed/Export Direct to Android unityLibrary (Compressed LZ4HC)")]
    public static void ExportToFlutterApp()
    {
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);

        var options = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            target = BuildTarget.Android,
            locationPathName = @"C:\MVAR\molecularvisionAR\android\unityLibrary",
            options = BuildOptions.CompressWithLz4HC
        };

        Debug.Log($"[FlutterEmbed] Exporting compressed build to {options.locationPathName}...");
        new ProjectExporterAndroid().Export(options, new List<string>());
    }

    [MenuItem("Flutter Embed/Export to Staged Directory (Compressed LZ4HC)")]
    public static void ExportStaged()
    {
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);

        var options = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            target = BuildTarget.Android,
            locationPathName = @"C:\MVAR\molecularvisionAR\unity-export-stage\android\unityLibrary",
            options = BuildOptions.CompressWithLz4HC
        };

        Debug.Log($"[FlutterEmbed] Exporting compressed staged build to {options.locationPathName}...");
        new ProjectExporterAndroid().Export(options, new List<string>());
    }
}
