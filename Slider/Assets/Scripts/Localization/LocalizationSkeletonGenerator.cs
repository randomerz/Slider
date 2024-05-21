using System.IO;
using Localization;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
   
   public LocalizationProjectConfiguration Configuration
   {
       set => configuration = value;
   }
   private LocalizationProjectConfiguration configuration;

   private string referenceLocalizationPath = null;
   private static string referenceLocalizationPathPreference;
   private static string saveLocalizationOutsidePathPreference;

   private void OnEnable()
   {
       titleContent = new GUIContent("Custom Build Settings");
       referenceLocalizationPath = EditorPrefs.GetString(referenceLocalizationPathPreference, null);
   }

   private void OnGUI()
   {
       configuration = EditorGUILayout.ObjectField(configuration, typeof(LocalizationProjectConfiguration), false) as LocalizationProjectConfiguration;

       if (GUILayout.Button("Select reference localization path"))
       {
           referenceLocalizationPath =
               EditorUtility.OpenFolderPanel("Reference localization", referenceLocalizationPath, null);
           EditorPrefs.SetString(referenceLocalizationPathPreference, referenceLocalizationPath);
       }

       if (referenceLocalizationPath != null)
       {
           var subdirs = LocalizationFile.LocaleList(referenceLocalizationPath);
           GUILayout.Label(string.Join('\n', subdirs));
       }
       
       if (GUILayout.Button("Generate localization INSIDE project"))
       {
           // Remove this if you don't want to close the window when starting a build
           GenerateSkeleton(configuration, referenceRoot: referenceLocalizationPath);
       }
       
       if (GUILayout.Button("Generate localization OUTSIDE project "))
       {
           // Remove this if you don't want to close the window when starting a build
           var saveLocalizationDir = EditorUtility.OpenFolderPanel(
               "Save localizations at", 
               EditorPrefs.GetString(saveLocalizationOutsidePathPreference), 
               null);
           
           EditorPrefs.SetString(saveLocalizationOutsidePathPreference, saveLocalizationDir);
           GenerateSkeleton(configuration, saveLocalizationDir, referenceRoot: referenceLocalizationPath);
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

   private void GenerateSkeleton(LocalizationProjectConfiguration projectConfiguration, string root = null, string referenceRoot = null)
   {
       string startingScenePath = EditorSceneManager.GetSceneAt(0).path; // EditorSceneManager always have 1 active scene (the opened scene)

       foreach (var locale in projectConfiguration.InitialLocales)
       {
           LocalizableContext localeGlobalConfig = LocalizableContext.ForSingleLocale(locale);
           string serializedConfigs = localeGlobalConfig.Serialize(serializeConfigurationDefaults: true, referenceFile: null);
           WriteFileAndForceParentPath(LocalizationFile.LocaleGlobalFilePath(locale.name, root), serializedConfigs);
       }
       
       foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes)
       {
           if (!editorBuildSettingsScene.enabled)
           {
               continue;
           }
           
           var scene = EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
           var skeleton = LocalizableContext.ForSingleScene(scene);
           
           // Default locale is covered in locale list
           // WriteFileAndForceParentPath(LocalizationFile.DefaultLocaleAssetPath(scene, root), serializedSkeleton);
           foreach (var locale in projectConfiguration.InitialLocales)
           {
               string serializedSkeleton;
               // If locale is English, don't bother migrating old translations
               // Otherwise, if an older translation exists, try migrate it
               if (
                   referenceRoot == null
                   || locale.name.Equals(LocalizationFile.DefaultLocale)
                   // || !File.Exists(LocalizationFile.LocaleAssetPath(locale.name, scene, referenceRoot)) // this is checked in factory method!
                   )
               {
                   serializedSkeleton = skeleton.Serialize(serializeConfigurationDefaults: false, referenceFile: null);
               }
               else
               {
                   serializedSkeleton = skeleton.Serialize(serializeConfigurationDefaults: false, referenceFile: LocalizationFile.MakeLocalizationFile(
                        locale.name,
                        LocalizationFile.LocaleAssetPath(locale.name, scene, referenceRoot))
                       );
               }
               WriteFileAndForceParentPath(LocalizationFile.LocaleAssetPath(locale.name, scene, root), serializedSkeleton);
           }
       }

       EditorSceneManager.OpenScene(startingScenePath);
   }

   private static void WriteFileAndForceParentPath(string path, string content)
   {
       var parent = Directory.GetParent(path);
       Directory.CreateDirectory(parent.FullName);
       using var file = File.Exists(path)
           ? new FileStream(path, FileMode.Truncate)
           : new FileStream(path, FileMode.CreateNew);
       StreamWriter sw = new(file);
       sw.Write(content);
       sw.Flush();
       sw.Close();
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
