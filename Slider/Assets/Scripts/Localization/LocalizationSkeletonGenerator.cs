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
   private bool saveLocalizationsElsewhere;
   private string saveLocalizationDir;

   private void OnEnable()
   {
       titleContent = new GUIContent("Custom Build Settings");
   }

   private void OnGUI()
   {
       var sle = GUILayout.Toggle(saveLocalizationsElsewhere, "Also save localization outside project");
       if (sle && !saveLocalizationsElsewhere)
       {
           saveLocalizationDir = EditorUtility.OpenFolderPanel("Save localizations at", saveLocalizationDir, null);
       }
       saveLocalizationsElsewhere = sle;
       
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
       string startingScenePath = EditorSceneManager.GetSceneAt(0).path;
       
       foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes)
       {
           if (!editorBuildSettingsScene.enabled)
           {
               continue;
           }
           
           Scene scene = EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
           LocalizableScene skeleton = new(scene);

           string serializedSkeleton = skeleton.Serialize();
           
           WriteFileAndForceParentPath(LocalizationFile.DefaultAssetPath(scene), serializedSkeleton);

           if (saveLocalizationsElsewhere)
           {
               WriteFileAndForceParentPath(Path.Join(saveLocalizationDir, LocalizationFile.LocalizationFileName(scene)), serializedSkeleton);
           }
       }

       EditorSceneManager.OpenScene(startingScenePath);
   }

   private static void WriteFileAndForceParentPath(string path, string content)
   {
       var parent = Directory.GetParent(path);
       Directory.CreateDirectory(parent.FullName);
       var stream = File.Exists(path) ? new FileStream(path, FileMode.Truncate) : new FileStream(path, FileMode.CreateNew);
       StreamWriter sw = new(stream);
       sw.Write(content);
       sw.Flush();
       sw.Close();
       stream.Close();
   }

   private void DoBuild()
   {
       var buildPlayerOptions = new BuildPlayerOptions
       {
           options = buildOptions
       };
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
