using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
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

   private void OnEnable()
   {
       titleContent = new GUIContent("Custom Build Settings");
   }

   private void OnGUI()
   {
       // add your GUI controls to modify the build here
       
       // TODO
       if (GUILayout.Button("generate localization"))
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
       string dir = EditorUtility.OpenFolderPanel("Pick Localization Skeleton Directory", "", "");
       
       for (int sceneBuildIndex = 0; sceneBuildIndex < EditorBuildSettings.scenes.Length; sceneBuildIndex++)
       {
           LocalizationHelpers.LocalizationFile skeleton = new();
       
           Dictionary<Type, Action<LocalizationHelpers.Localizable>> localizationMapping = new()
           {
               { typeof(TMP_Text), localizable => skeleton.AddEntryTmp(localizable) }
           };
           
           EditorBuildSettingsScene editorBuildSettingsScene = EditorBuildSettings.scenes[sceneBuildIndex];
           if (!editorBuildSettingsScene.enabled)
           {
               continue;
           }
           
           Scene scene = EditorSceneManager.OpenScene(editorBuildSettingsScene.path);

           LocalizationHelpers.IterateLocalizableTypes(scene, localizationMapping);
           
           File.WriteAllText(Path.Join(dir, scene.name + ".csv"), skeleton.Serialize());
       }
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
