using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Localization;
using UnityEngine;

#if UNITY_EDITOR
using System.Text;
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

   private static LocalizationSkeletonGenerator OpenCustomBuildWindow(BuildPlayerOptions buildPlayerOptions)
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
   public static string referenceLocalizationPathPreference;
   private static string saveLocalizationOutsidePathPreference;
   private IEnumerable<string> referenceLocales = null;

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

           referenceLocales = null;
       }
       GUILayout.Label("^ Reference localization includes old translations that will be migrated into newly generated localization CSV files");
       
       string referenceDescription = "(No reference translation selected)";
       if (referenceLocalizationPath != null)
       {
           referenceDescription = $"reference localization: [{referenceLocalizationPath}]\nwhich contains...";
           if (referenceLocales == null)
           {
               referenceLocales =
                   LocalizationFile.LocaleList(LocalizationFile.DefaultLocale, referenceLocalizationPath);
           }
           else
           {
               foreach (string subdir in referenceLocales)
               {
                   referenceDescription += $"\n - {subdir}";
               }
           }
       }
       GUILayout.Label(referenceDescription);
       
       if (GUILayout.Button("Generate localization INSIDE project"))
       {
           // Remove this if you don't want to close the window when starting a build
           
           if (EditorUtility.DisplayDialog("Confirm",
                   $"Generate localization skeletons within the project?\nThis may overwrite existing content!",
                   "Yes", "No"))
           {
               var copyFullyUpdatedCsvBackToProj = GenerateSkeleton(configuration, referenceRoot: referenceLocalizationPath, isDev: true);
               copyFullyUpdatedCsvBackToProj();
           }
       }
       GUILayout.Label("^ generates a relevant CSV files to the StreamingAssets folder, following configurations set in the localization project configuration object. Such files will be directly copied into the build!");
       
       if (GUILayout.Button("Generate localization OUTSIDE project "))
       {
           // Remove this if you don't want to close the window when starting a build
           var saveLocalizationDir = EditorUtility.OpenFolderPanel(
               "Save localizations at", 
               EditorPrefs.GetString(saveLocalizationOutsidePathPreference), 
               null);

           if (EditorUtility.DisplayDialog("Confirm",
                   $"Generate localization skeletons at {saveLocalizationDir}?\nThis may overwrite existing content!",
                   "Yes", "No"))
           {
               EditorPrefs.SetString(saveLocalizationOutsidePathPreference, saveLocalizationDir);
               var copyFullyUpdatedCsvBackToRoot = GenerateSkeleton(configuration, root: saveLocalizationDir, referenceRoot: referenceLocalizationPath, isDev: true);
               copyFullyUpdatedCsvBackToRoot();
           }
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
           // Debug.Log(localeIsValid);
       }

       return localeIsValid;
   }
   
   // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
   private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
   {
       // Get information about the source directory
       var dir = new DirectoryInfo(sourceDir);

       // Check if the source directory exists
       if (!dir.Exists)
           throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

       // Cache directories before we start copying
       var dirs = dir.GetDirectories();

       // Create the destination directory
       Directory.CreateDirectory(destinationDir);

       // Get the files in the source directory and copy to the destination directory
       foreach (var file in dir.GetFiles())
       {
           var targetFilePath = Path.Combine(destinationDir, file.Name);
           file.CopyTo(targetFilePath);
       }

       // If recursive and copying subdirectories, recursively call this method
       if (!recursive)
       {
           return;
       }
       
       foreach (var subDir in dirs)
       {
           var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
           CopyDirectory(subDir.FullName, newDestinationDir, true);
       }
   }
   
   /// <summary>
   /// reference file = old translations that should be considered for migration into new translations
   /// for migration to be considered the scene must exactly match (or in the case of locale config, the
   /// locale has to match
   /// </summary>
   /// <param name="locale"></param>
   /// <param name="path"></param>
   /// <returns></returns>
   private static LocalizationFile NullifyReferenceRootIfNeeded(LocaleConfiguration locale, string path)
   {
       // If locale is English, don't bother migrating old translations
       // Otherwise, if an older translation exists, try migrate it
       if (locale.name == LocalizationFile.DefaultLocale || locale.name == LocalizationFile.TestingLanguage) {
            return null;
       }
       else if(!File.Exists(path))
       {
           Debug.LogError($"{path} does not exist!");
           return null;
       }

       var (referenceFile, err) = LocalizationFile.MakeLocalizationFile(locale.name, path).Result;
       if (referenceFile == null)
       {
           LocalizationFile.PrintParserError(err, path);
       }

       return referenceFile;
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="dst1Path">a csv file path this function will definitely write to, typically a temp file location</param>
   /// <param name="dst2Path">an in-project path to write to, used only when a locale needs to be embedded within a build
   /// (from GameBuilder!)</param>
   /// <param name="doWrite"></param>
   /// <param name="copyIf"></param>
   private static void WriteAndCopyIf(string dst1Path, string dst2Path, Action<TextWriter> doWrite, bool copyIf)
   {
       using (var file = File.Exists(dst1Path)
                  ? new FileStream(dst1Path, FileMode.Truncate)
                  : new FileStream(dst1Path, FileMode.CreateNew))
       {
           using (var tw = new StreamWriter(file, Encoding.UTF8))
           {
               doWrite(tw);
           }
       }

       if (copyIf)
       {
           File.Copy(dst1Path, dst2Path);
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

       void CleanCreate(string dir)
       {
           if (Directory.Exists(dir))
           {
               Directory.Delete(dir);
           }

           Directory.CreateDirectory(dir);
       }
       
       string tempDirectory = Path.Combine(Path.GetTempPath(), "__slider_localization_external_save_dir__");
       CleanCreate(tempDirectory);

       if (root == referenceRoot)
       {
           var resolvedReferenceRoot = LocalizationFile.LocalizationRootPath(root);
           string tempDirectory2 = Path.Combine(Path.GetTempPath(), "__slider_localization_external_ref_dir__");
           CleanCreate(tempDirectory2);
           CopyDirectory(resolvedReferenceRoot, tempDirectory2, true);
           referenceRoot = tempDirectory2;
       }
       
       string startingScenePath = EditorSceneManager.GetSceneAt(0).path; // EditorSceneManager always have 1 active scene (the opened scene)

       var shapes = AssetDatabase.FindAssets("t:Shape")
           .Select(AssetDatabase.GUIDToAssetPath)
           .Select(AssetDatabase.LoadAssetAtPath<Shape>);

       var globalStrings = 
           shapes.ToDictionary(s => LocalizableContext.JungleShapeToPath(s.shapeName), s => s.shapeName);

       foreach (var kv in Areas.DiscordNames)
       {
           globalStrings.Add(LocalizableContext.AreaToDiscordNamePath(kv.Key), kv.Value);
       }
       
       foreach (var kv in Areas.DisplayNames)
       {
           globalStrings.Add(LocalizableContext.AreaToDisplayNamePath(kv.Key), kv.Value);
       }

       if (root == null)
       {
           var inProjectDirectory = LocalizationFile.LocalizationFolderPath();
           if (Directory.Exists(inProjectDirectory))
           {
               Directory.Delete(inProjectDirectory, true);
           }
           Directory.CreateDirectory(inProjectDirectory);
       }

       foreach (var prefab in projectConfiguration.RelevantPrefabs)
       {
           var injector = prefab.GetComponent<LocalizationInjector>();
           var skeleton = LocalizableContext.ForInjector(injector);
           
           foreach (var locale in projectConfiguration.InitialLocales)
           {
               WriteAndCopyIf(
                   ForceParentPath(LocalizationFile.AssetPath(locale.name, injector, tempDirectory)),
                   ForceParentPath(LocalizationFile.AssetPath(locale.name, injector)),
                   (tw) => skeleton.Serialize(
                       serializeConfigurationDefaults: false,
                       tw: tw,
                       referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.AssetPath(locale.name, injector, referenceRoot)),
                       autoPadTranslated: locale.name == LocalizationFile.TestingLanguage ? "_ho_"  : null
                       ),
                   localeIsValid[locale.name] && root == null);
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

           foreach (var kv in skeleton.GlobalStringsToExport)
           {
               if (!globalStrings.TryAdd(kv.Key, kv.Value))
               {
                   Debug.LogWarning($"Duplicate global export string {kv.Key}: {kv.Value}, all occurrences will use shared translation");
               }
           }
           
           // Default locale is covered in locale list
           // WriteFileAndForceParentPath(LocalizationFile.DefaultLocaleAssetPath(scene, root), serializedSkeleton);
           foreach (var locale in projectConfiguration.InitialLocales)
           {
               WriteAndCopyIf(
                   ForceParentPath(LocalizationFile.AssetPath(locale.name, scene, tempDirectory)),
                   ForceParentPath(LocalizationFile.AssetPath(locale.name, scene)),
                   (tw) => skeleton.Serialize(
                       serializeConfigurationDefaults: false,
                       tw: tw,
                       referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.AssetPath(locale.name, scene, referenceRoot)),
                       autoPadTranslated: locale.name == LocalizationFile.TestingLanguage ? "_har_"  : null
                   ),
                   localeIsValid[locale.name] && root == null);
           }
       }
       
       foreach (var locale in projectConfiguration.InitialLocales)
       {
           LocalizableContext skeleton = LocalizableContext.ForSingleLocale(locale, globalStrings);
           
           WriteAndCopyIf(
               ForceParentPath(LocalizationFile.LocaleGlobalFilePath(locale.name, tempDirectory)),
               ForceParentPath(LocalizationFile.LocaleGlobalFilePath(locale.name)),
               (tw) => skeleton.Serialize(
                   serializeConfigurationDefaults: true,
                   tw: tw,
                   referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.LocaleGlobalFilePath(locale.name, referenceRoot)),
                   autoPadTranslated: locale.name == LocalizationFile.TestingLanguage ? "_yarr_"  : null
               ),
               localeIsValid[locale.name] && root == null);
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

       return ForceCopyEverythingToDst2; // this is called after GameBuilder finishes exporting the builds,
                                         // and it's safe to edit the StreamingAssets folder inside the project again

       void ForceCopyEverythingToDst2()
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
       }
   }

   private static void GuardedDeleteFile(string path)
   {
       if (File.Exists(path))
       {
           File.Delete(path);
       }
   }

   private static string ForceParentPath(string path)
   {
       var parent = Directory.GetParent(path);
       Directory.CreateDirectory(parent.FullName);
       return path;
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