using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Localization;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
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
       configuration = LocalizationProjectConfiguration.ScriptableObjectSingleton;
   }

   private void OnGUI()
   {
       configuration = EditorGUILayout.ObjectField(configuration, typeof(LocalizationProjectConfiguration), false) as LocalizationProjectConfiguration;
       GUILayout.Label("You need to select the scriptable object that configures the project's localization, in order to use the localization generation stuff below");
       
       if (GUILayout.Button("Select reference localization path"))
       {
           referenceLocalizationPath =
               EditorUtility.OpenFolderPanel("Reference localization", referenceLocalizationPath, null);
           EditorPrefs.SetString(referenceLocalizationPathPreference, referenceLocalizationPath);
       }
       GUILayout.Label("^ Reference localization includes old translations that will be migrated into newly generated localization CSV files");
       
       if (referenceLocalizationPath != null)
       {
           GUILayout.Label($"reference localization: [{referenceLocalizationPath}]\nwhich contains...");
           var subdirs = LocalizationFile.LocaleList(LocalizationFile.DefaultLocale, referenceLocalizationPath);
           foreach (string subdir in subdirs)
           {
               GUILayout.Label($" - {subdir}");
           }
       }
       
       if (GUILayout.Button("Generate localization INSIDE project"))
       {
           // Remove this if you don't want to close the window when starting a build
           var Apply = GenerateSkeleton(configuration, referenceRoot: referenceLocalizationPath);
           Apply();
       }
       GUILayout.Label("^ generates a relevant CSV files to the StreamingAssets folder, following configurations set in the localization project configuration object. Such files will be directly copied into the build!");
       
       if (GUILayout.Button("Generate localization OUTSIDE project "))
       {
           // Remove this if you don't want to close the window when starting a build
           var saveLocalizationDir = EditorUtility.OpenFolderPanel(
               "Save localizations at", 
               EditorPrefs.GetString(saveLocalizationOutsidePathPreference), 
               null);
           
           EditorPrefs.SetString(saveLocalizationOutsidePathPreference, saveLocalizationDir);
           var Apply = GenerateSkeleton(configuration, root: saveLocalizationDir, referenceRoot: referenceLocalizationPath);
           Apply();
       }
       GUILayout.Label("^ generates a relevant CSV files to any selected folder, following configurations set in the localization project configuration object");
       
       // Build button
       string buildButtonLabel = (buildOptions & BuildOptions.AutoRunPlayer) == 0 ? "Build" : "Build And Run";
       if (GUILayout.Button(buildButtonLabel))
       {
           // Remove this if you don't want to close the window when starting a build
           Close();

           DoBuild();
       }
       GUILayout.Label("^ builds and runs the project using the current build setting, no localization function associated with it");

   }

   static Dictionary<string, bool> GetLocaleValidityMap(LocalizationProjectConfiguration projectConfiguration)
   {
        Dictionary<string, bool> localeIsValid = new();
        foreach (var locale in projectConfiguration.InitialLocales)
        {
               bool isValid = true;
               foreach (var option in locale.options)
               {
                   if (option.name == LocalizationFile.Config.IsValid)
                   {
                       if (!int.TryParse(option.value, out int isValidFlag) || isValidFlag != 1)
                       {
                           isValid = false;
                           break;
                       }
                   }
               }

               localeIsValid[locale.name] = isValid;
               Debug.Log(localeIsValid);
        }

        return localeIsValid;
   }
   
   // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
   static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
   {
       // Get information about the source directory
       var dir = new DirectoryInfo(sourceDir);

       // Check if the source directory exists
       if (!dir.Exists)
           throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

       // Cache directories before we start copying
       DirectoryInfo[] dirs = dir.GetDirectories();

       // Create the destination directory
       Directory.CreateDirectory(destinationDir);

       // Get the files in the source directory and copy to the destination directory
       foreach (FileInfo file in dir.GetFiles())
       {
           string targetFilePath = Path.Combine(destinationDir, file.Name);
           file.CopyTo(targetFilePath);
       }

       // If recursive and copying subdirectories, recursively call this method
       if (recursive)
       {
           foreach (DirectoryInfo subDir in dirs)
           {
               string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
               CopyDirectory(subDir.FullName, newDestinationDir, true);
           }
       }
   }

   /// <summary>
   /// Generates CSV files for relevant locales, may create a temporary directory in the process. Return value must be taken
   /// care of when Streaming Assets folder is no longer being used.
   /// </summary>
   /// <param name="projectConfiguration"></param>
   /// <param name="root">set to `null` to refer to the Streaming Assets folder within this project; destination to write all CSV files</param>
   /// <param name="referenceRoot">Path to an existing set of translations, can be same as root</param>
   /// <param name="isDev">Generate all locales regardless of validity (the IsValid in project config)</param>
   /// <returns>
   /// Action that must be run after build is done (i.e. streaming assets folder can be edited again without disturbing the build).
   /// Namely copies all CSV files (including the skipped, dev-mode locales into the root folder).
   /// </returns>
   public static Action GenerateSkeleton(
       LocalizationProjectConfiguration projectConfiguration, string root = null, string referenceRoot = null, bool isDev = true)
   {
       Dictionary<string, bool> localeIsValid = isDev ? 
           projectConfiguration.InitialLocales.ToDictionary((config) => config.name, _ => true) 
           : GetLocaleValidityMap(projectConfiguration);
       
       string startingScenePath = EditorSceneManager.GetSceneAt(0).path; // EditorSceneManager always have 1 active scene (the opened scene)

       string tempDirectory = Path.Combine(Path.GetTempPath(), "__slider_localization_external_save_dir__");
       if (Directory.Exists(tempDirectory)) {
           Directory.Delete(tempDirectory, true);
       }
       Directory.CreateDirectory(tempDirectory);
       Action returnAction = () =>
       {
           string dest = LocalizationFile.LocalizationFolderPath(root);
           string src = LocalizationFile.LocalizationFolderPath(tempDirectory);
           if (Directory.Exists(dest))
           {
               Directory.Delete(dest, true);
           }
           
           // copy & delete will work across volumes whereas directory.move does not
           CopyDirectory(src, dest, true);
           Directory.Delete(src, true);
       };

       string inProjectDirectory = root == null ? LocalizationFile.LocalizationFolderPath() : null;
       if (root == null)
       {
           if (Directory.Exists(inProjectDirectory))
           {
               Directory.Delete(inProjectDirectory, true);
           }
           Directory.CreateDirectory(inProjectDirectory);
       }
       
       foreach (var locale in projectConfiguration.InitialLocales)
       {
           LocalizableContext localeGlobalConfig = LocalizableContext.ForSingleLocale(locale);
           string serializedConfigs = localeGlobalConfig.Serialize(serializeConfigurationDefaults: true, referenceFile: null);
           
           WriteFileAndForceParentPath(LocalizationFile.LocaleGlobalFilePath(locale.name, tempDirectory), serializedConfigs);
           if (localeIsValid[locale.name] && root == null)
           {
               WriteFileAndForceParentPath(LocalizationFile.LocaleGlobalFilePath(locale.name), serializedConfigs);
           }
       }

       // AT: note: reference file = old translations that should be considered for migration into new translations
       //           for migration to be considered the scene must exactly match (or in the case of locale config, the
       //           locale has to match
       // wtf nested functions exist???
       LocalizationFile NullifyReferenceRootIfNeeded(LocaleConfiguration locale, string path)
       {
           // If locale is English, don't bother migrating old translations
           // Otherwise, if an older translation exists, try migrate it
           if (locale.name.Equals(LocalizationFile.DefaultLocale) || !File.Exists(path))
           {
               return null;
           }

           var (referenceFile, err) = LocalizationFile.MakeLocalizationFile(locale.name, path);
           if (referenceFile == null)
           {
               LocalizationFile.PrintParserError(err, path);
           }

           return referenceFile;
       }


       foreach (var prefab in projectConfiguration.RelevantPrefabs)
       {
           var skeleton = LocalizableContext.ForSinglePrefab(prefab);
           
           foreach (var locale in projectConfiguration.InitialLocales)
           {
               var serializedSkeleton = skeleton.Serialize(
                   serializeConfigurationDefaults: false,
                   referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.AssetPath(locale.name, prefab, referenceRoot))
               );

               WriteFileAndForceParentPath(LocalizationFile.AssetPath(locale.name, prefab, tempDirectory), serializedSkeleton);
               
               if (localeIsValid[locale.name] && root == null)
               {
                   WriteFileAndForceParentPath(LocalizationFile.AssetPath(locale.name, prefab), serializedSkeleton);
               }
           }
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
               var serializedSkeleton = skeleton.Serialize(
                   serializeConfigurationDefaults: false,
                   referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.AssetPath(locale.name, scene, referenceRoot))
               );
               
               WriteFileAndForceParentPath(LocalizationFile.AssetPath(locale.name, scene, tempDirectory), serializedSkeleton);
               if (localeIsValid[locale.name] && root == null)
               {
                   WriteFileAndForceParentPath(LocalizationFile.AssetPath(locale.name, scene), serializedSkeleton);
               }
           }
       }

       try
       {
           // AT: patch "scene not found" when running from GameBuilder
           EditorSceneManager.OpenScene(startingScenePath);
       }
       catch (Exception e)
       {
           Debug.LogWarning(e);
       }

       return returnAction;
   }

   private static void GuardedDeleteFile(string path)
   {
       if (File.Exists(path))
       {
           File.Delete(path);
       }
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

#endif