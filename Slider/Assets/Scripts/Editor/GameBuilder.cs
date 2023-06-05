using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

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

        BuildPlayer(BuildTarget.StandaloneWindows64, buildRootPath, filename);
        BuildPlayer(BuildTarget.StandaloneOSX, buildRootPath, filename);
        BuildPlayer(BuildTarget.StandaloneLinux64, buildRootPath, filename);
    }

    // For use in Unity
    [MenuItem("File/GameBuilder/Build Windows + Mac OSX + Linux")]
    public static void UnityBuildAll()
    {
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
            Debug.LogError("Path must be provided!");
            return;
        }
        if (filename == null || filename.Length == 0)
        {
            Debug.LogError("Filename must be provided!");
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
        Debug.Log($"====== Building: {buildTarget.ToString()} ======");

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build successful - Build written to {options.locationPathName}");
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            Debug.LogError($"Build failed");
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
        Debug.Log($"Building {scenes.Count} scenes...");
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
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(s);
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
