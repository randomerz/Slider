using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using NUnit.Framework;

/// <summary>
/// Builder for use in CLI. See `build_unity.bat` outside of the project folder for example usage.
/// </summary>
public class GameBuilder
{
    // To be called by CLI -- you can't run it if editor is open
    public static void BuildAllPlatforms()
    {
        string buildRootPath = GetArg("buildRootPath");
        string filename = GetArg("filename");

        Debug.Log($"Build Root Path: {buildRootPath}");

        BuildPlayer(BuildTarget.StandaloneWindows64, buildRootPath, filename);
        BuildPlayer(BuildTarget.StandaloneOSX, buildRootPath, filename);
        BuildPlayer(BuildTarget.StandaloneLinux64, buildRootPath, filename);
    }

    // For use in Unity
    [MenuItem("File/GameBuilder/Build Windows + Mac OSX + Linux")]
    public static void UnityBuildAll()
    {
        // AT: force refresh English locale files to avoid showing stale text
        var config = LocalizationProjectConfiguration.ScriptableObjectSingleton;
        LocalizationSkeletonGenerator.GenerateSkeleton(config, LocalizationSkeletonGenerator.GenerateSkeletonStrategy.OnlyDefaultEnglishLocale);
        
        string buildRootPath = EditorUtility.SaveFolderPanel(
            "Select the ROOT folder for your builds...",
            GetProjectFolderPath(),
            ""
        );
        if (buildRootPath == "")
        {
            return;
        }

        string filename = GetProjectName();

        BuildPlayer(BuildTarget.StandaloneWindows64, buildRootPath, filename);
        BuildPlayer(BuildTarget.StandaloneOSX, buildRootPath, filename);
        BuildPlayer(BuildTarget.StandaloneLinux64, buildRootPath, filename);
    }

    // this is the main player builder function
    private static void BuildPlayer(BuildTarget buildTarget, string buildRootPath, string filename)
    {
        if (buildRootPath == null || buildRootPath.Length == 0)
        {
            Debug.LogError("[Builds] Path must be provided!");
            return;
        }
        if (filename == null || filename.Length == 0)
        {
            Debug.LogError("[Builds] Filename must be provided!");
            return;
        }

        string fileExtension = "";
        string modifier = "";

        // configure path variables based on the platform we're targeting
        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                modifier = "windows";
                fileExtension = ".exe";
                break;
            case BuildTarget.StandaloneOSX:
                modifier = "mac";
                fileExtension = ".app";
                break;
            case BuildTarget.StandaloneLinux64:
                modifier = "linux";
                fileExtension = ".x86_64";
                break;
        }

        string buildPath = System.IO.Path.Join(buildRootPath, modifier); // path/to/build/windows
        string finalPath = System.IO.Path.Join(buildPath, filename + fileExtension); // path/to/build/windows/Game.exe

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenePaths(),
            target = buildTarget,
            locationPathName = finalPath,
        };
        Debug.Log($"[Builds] Building: {buildTarget.ToString()} ======");

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Builds] Build successful - Build written to {options.locationPathName}");
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[Builds] Build failed");
        }

        DeleteBurstDebugInformationFolder(report);
    }

    private static void DeleteBurstDebugInformationFolder(BuildReport buildReport)
    {
        string outputPath = buildReport.summary.outputPath;

        try
        {
            string applicationName = System.IO.Path.GetFileNameWithoutExtension(outputPath);
            string outputFolder = System.IO.Path.GetDirectoryName(outputPath);
            Assert.IsNotNull(outputFolder);

            outputFolder = System.IO.Path.GetFullPath(outputFolder);

            string burstDebugInformationDirectoryPath = System.IO.Path.Combine(outputFolder, $"{applicationName}_BurstDebugInformation_DoNotShip");

            if (Directory.Exists(burstDebugInformationDirectoryPath))
            {
                Debug.Log($"[Builds] Deleting Burst debug information folder at path '{burstDebugInformationDirectoryPath}'...");

                Directory.Delete(burstDebugInformationDirectoryPath, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Builds] An unexpected exception occurred while performing build cleanup: {e}");
        }
    }

    private static string[] GetScenePaths()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }

    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string GetProjectFolderPath()
    {
        string s = Application.dataPath; // returns Root/Project/Assets/
        DirectoryInfo di = new DirectoryInfo(s);
        s = di.Parent.Parent.FullName;
        return s;
    }

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-" + name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
