using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Scene management helpers
	/// </summary>
	public class MMScene  
	{
		/// <summary>
		/// Returns an array filled with all the currently loaded scenes
		/// </summary>
		/// <returns></returns>
		public static Scene[] GetLoadedScenes()
		{
			int sceneCount = SceneManager.sceneCount;

			List<Scene> loadedScenes = new List<Scene>(sceneCount);
			// Scene[] loadedScenes = new Scene[sceneCount];
 
			for (int i = 0; i < sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene.isLoaded)
				{
					loadedScenes.Add(scene);
				}
				else
				{
					Debug.LogWarning($"{scene.name} NOT LOADED");
				}
			}

			return loadedScenes.ToArray();
		}

		/// <summary>
		/// Returns a list of all the scenes present in the build
		/// </summary>
		/// <returns></returns>
		public static List<string> GetScenesInBuild()
		{
			List<string> scenesInBuild = new List<string>();
            
			for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
			{
				string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
				int lastSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
				scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".", StringComparison.Ordinal) - lastSlash - 1));
			}

			return scenesInBuild;
		}

		/// <summary>
		/// Returns true if a scene by the specified name is present in the build
		/// </summary>
		/// <param name="sceneName"></param>
		/// <returns></returns>
		public static bool SceneInBuild(string sceneName)
		{
			return GetScenesInBuild().Contains(sceneName);
		}
	}
}