using System.IO;
using Localization;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// https://forum.unity.com/threads/custom-build-settings-window.1185259/
public class LocalizationSkeletonGenerator : EditorWindow
{
   [InitializeOnLoadMethod]
   private static void InitOnLoad()
   {
       // hijack the Build button in Unity's Build Settings window
       BuildPlayerWindow.RegisterGetBuildPlayerOptionsHandler(OnGetBuildPlayerOptions);
       BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);
   }

   private static BuildPlayerOptions OnGetBuildPlayerOptions(BuildPlayerOptions buildPlayerOptions)
   {
       OpenCustomBuildWindow(buildPlayerOptions);
       return buildPlayerOptions;
   }

   private static void OnBuildPlayer(BuildPlayerOptions buildPlayerOptions)
   {
       // Do nothing here as the build is triggered from the Build button of the custom window
   }

   public static LocalizationSkeletonGenerator OpenCustomBuildWindow(BuildPlayerOptions buildPlayerOptions)
   {
       var w = EditorWindow.GetWindow<LocalizationSkeletonGenerator>();
       // here you can store initial options from Unity's Build window, such as if "Build" or "Build And Run" was pressed
       w.buildOptions = buildPlayerOptions.options;
       w.Show();
       
       return w;
   }

   private BuildOptions buildOptions;
   private bool saveDebug;
   private string saveDebugSceneName = "";

   private void OnEnable()
   {
       titleContent = new GUIContent("Custom Build Settings");
   }

   private void OnGUI()
   {
       // add your GUI controls to modify the build here
       saveDebugSceneName = EditorSceneManager.GetSceneAt(0).name;
       saveDebug = GUILayout.Toggle(saveDebug, $"Save debug localization for currently opened scene { saveDebugSceneName }");
       GUILayout.Label($"There is at most one debug localization at a time! It will be saved at { LocalizationFile.DebugAssetPath }");
       
       if (GUILayout.Button("Generate localization"))
       {
           // Remove this if you don't want to close the window when starting a build

           GenerateSkeleton();
       }
       
       // Build button
       string buildButtonLabel = (buildOptions & BuildOptions.AutoRunPlayer) == 0 ? "Build" : "Build And Run";
       if (GUILayout.Button(buildButtonLabel))
       {
           // Remove this if you don't want to close the window when starting a build
           Close();

           DoBuild();
       }
   }

   private void GenerateSkeleton()
   {
       string dir = EditorUtility.OpenFolderPanel("Save Localization Skeleton At Directory", "", "");

       string startingScenePath = EditorSceneManager.GetSceneAt(0).path;
       
       for (int sceneBuildIndex = 0; sceneBuildIndex < EditorBuildSettings.scenes.Length; sceneBuildIndex++)
       {
           EditorBuildSettingsScene editorBuildSettingsScene = EditorBuildSettings.scenes[sceneBuildIndex];
           if (!editorBuildSettingsScene.enabled)
           {
               continue;
           }
           
           Scene scene = EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
           LocalizableScene skeleton = new(scene);

           string serializedSkeleton = skeleton.Serialize();
           
           WriteFileAndForceParentPath(Path.Join(dir, scene.name + "_localization.csv"), serializedSkeleton);

           if (saveDebug && saveDebugSceneName.Equals(scene.name))
           {
               WriteFileAndForceParentPath(LocalizationFile.DebugAssetPath, serializedSkeleton);
           }
           
       }

       EditorSceneManager.OpenScene(startingScenePath);
   }

   private static void WriteFileAndForceParentPath(string path, string content)
   {
       var parent = Directory.GetParent(path);
       Directory.CreateDirectory(parent.FullName);
       var stream = File.Create(path);
       StreamWriter sw = new(stream);
       sw.Write(content);
   }

   private void DoBuild()
   {
       var buildPlayerOptions = new BuildPlayerOptions();
       buildPlayerOptions.options = buildOptions;
       try
       {
           // This gets the default scene list and options from Unity's Build Settings window
           // This prompts for the build output location and stores it in buildPlayerOptions.locationPathName
           buildPlayerOptions = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(buildPlayerOptions);
       }
       catch (BuildPlayerWindow.BuildMethodException)
       {
           // Hide an exception from log if user cancels the build location prompt
           return;
       }

       // Here you can modify the buildPlayerOptions or the project with values set in the window

       // Execute the build (using the default build method in this example)
       BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
   }
}
