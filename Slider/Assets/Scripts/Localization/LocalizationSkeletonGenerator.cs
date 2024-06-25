using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Localization;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
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
           GenerateSkeleton(configuration, referenceRoot: referenceLocalizationPath);
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
           GenerateSkeleton(configuration, root: saveLocalizationDir, referenceRoot: referenceLocalizationPath);
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

   public enum GenerateSkeletonStrategy
   {
       AllLocales,
       OnlyDefaultEnglishLocale
   }

   public static void GenerateSkeleton(
       LocalizationProjectConfiguration projectConfiguration, GenerateSkeletonStrategy strategy = GenerateSkeletonStrategy.AllLocales, string root = null, string referenceRoot = null)
   {
       string startingScenePath = EditorSceneManager.GetSceneAt(0).path; // EditorSceneManager always have 1 active scene (the opened scene)

       Dictionary<string, string> globalStrings = new();
       
       var shapes = AssetDatabase.FindAssets("t:Shape")
           .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
           .Select(path => AssetDatabase.LoadAssetAtPath<Shape>(path));

       foreach (var s in shapes)
       {
           globalStrings.Add(SpecificTypeHelpers.JungleShapeToPath(s.shapeName), s.shapeName);
       }
       
       var collectibles = AssetDatabase
           .FindAssets("t:prefab")
           .Select(AssetDatabase.GUIDToAssetPath)
           .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
           .Where(go => go.GetComponent<Collectible>() != null);

       foreach (var c in collectibles)
       {
           var collectible = c.GetComponent<Collectible>();
           var cname = collectible.GetCollectibleData().name;
           
           // this is true only for variant roots
           if (string.IsNullOrWhiteSpace(cname))
           {
               continue;
           }
           
           globalStrings.Add(SpecificTypeHelpers.CollectibleToPath(cname, collectible.GetCollectibleData().area), cname);
       }

       foreach (var kv in Areas.DiscordNames)
       {
           globalStrings.Add(SpecificTypeHelpers.AreaToDiscordNamePath(kv.Key), kv.Value);
       }

       foreach (var kv in Areas.DisplayNames)
       {
           globalStrings.Add(SpecificTypeHelpers.AreaToDisplayNamePath(kv.Key), kv.Value);
       }

       // AT: note: reference file = old translations that should be considered for migration into new translations
       //           for migration to be considered the scene must exactly match (or in the case of locale config, the
       //           locale has to match
       // wtf nested functions exist???
       LocalizationFile NullifyReferenceRootIfNeeded(LocaleConfiguration locale, string path)
       {
           // If locale is English, don't bother migrating old translations
           // Otherwise, if an older translation exists, try migrate it
           if (locale.name == LocalizationFile.DefaultLocale || locale.name == LocalizationFile.GoofyAhLanguage || !File.Exists(path))
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
               WriteFileAndForceParentPath(LocalizationFile.AssetPath(locale.name, prefab, root), serializedSkeleton);
           }
       }
       
       IEnumerable<LocaleConfiguration> usedLocales;
       switch (strategy)
       {
           case GenerateSkeletonStrategy.AllLocales:
               usedLocales = projectConfiguration.InitialLocales.Where(_ => true);
               break;
           case GenerateSkeletonStrategy.OnlyDefaultEnglishLocale:
               usedLocales =
                   projectConfiguration.InitialLocales.Where(loc => loc.name.Equals(LocalizationFile.DefaultLocale));
               break;
           default:
               throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
       }

       var localeConfigurations = usedLocales.ToList();
       Debug.Log($"Generating CSV files for locales { string.Join(',', localeConfigurations.Select(loc => loc.name)) }");

       // lol
       string[] goofyAhhPirateSounds =
       {
           "_arrr_",
           "_ho_",
           "_yar_",
           "_ahoy_"
       };
       
       foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes)
       {
           if (!editorBuildSettingsScene.enabled)
           {
               continue;
           }
           
           var scene = EditorSceneManager.OpenScene(editorBuildSettingsScene.path);
           var skeleton = LocalizableContext.ForSingleScene(scene);

           foreach (var kv in skeleton.AdditionalExportedStrings)
           {
               if (!globalStrings.TryAdd(kv.Key, kv.Value))
               {
                   Debug.LogError($"Duplicate global export string {kv.Key}: {kv.Value}");
               }
           }
           
           // Default locale is covered in locale list
           // WriteFileAndForceParentPath(LocalizationFile.DefaultLocaleAssetPath(scene, root), serializedSkeleton);
           foreach (var locale in localeConfigurations)
           {
               var serializedSkeleton = skeleton.Serialize(
                   serializeConfigurationDefaults: false,
                   referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.AssetPath(locale.name, scene, referenceRoot)),
                   autoPadTranslated: locale.name == LocalizationFile.GoofyAhLanguage ? () => goofyAhhPirateSounds[Random.Range(0, goofyAhhPirateSounds.Length-1)]  : null
               );
               WriteFileAndForceParentPath(LocalizationFile.AssetPath(locale.name, scene, root), serializedSkeleton);
           }
       }
       
       // Write locale global files at last due to global strings being accumulated over the scan...
       foreach (var locale in projectConfiguration.InitialLocales)
       {
           LocalizableContext localeGlobalConfig = LocalizableContext.ForSingleLocale(locale, globalStrings);
           string serializedConfigs = localeGlobalConfig.Serialize(
               serializeConfigurationDefaults: true, 
               referenceFile: NullifyReferenceRootIfNeeded(locale, LocalizationFile.LocaleGlobalFilePath(locale.name, referenceRoot)),
               autoPadTranslated: locale.name == LocalizationFile.GoofyAhLanguage ? () => goofyAhhPirateSounds[Random.Range(0, goofyAhhPirateSounds.Length-1)] : null);
           WriteFileAndForceParentPath(LocalizationFile.LocaleGlobalFilePath(locale.name, root), serializedConfigs);
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