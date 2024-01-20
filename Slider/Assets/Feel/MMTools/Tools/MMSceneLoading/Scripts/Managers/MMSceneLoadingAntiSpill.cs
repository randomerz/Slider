using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This helper class, meant to be used by the MMAdditiveSceneLoadingManager, creates a temporary scene to store objects that might get instantiated, and empties it in the destination scene once loading is complete
	/// </summary>
	public class MMSceneLoadingAntiSpill
	{
		protected Scene _antiSpillScene;
		protected Scene _destinationScene;
		protected UnityAction<Scene, Scene> _onActiveSceneChangedCallback;
		protected string _sceneToLoadName;
		protected string _antiSpillSceneName;
		protected List<GameObject> _spillSceneRoots = new List<GameObject>(50);
		protected static List<string> _scenesInBuild;
		
		/// <summary>
		/// Creates the temporary scene
		/// </summary>
		/// <param name="sceneToLoadName"></param>
		public virtual void PrepareAntiFill(string sceneToLoadName, string antiSpillSceneName = "")
		{
			_destinationScene = default; 
			_sceneToLoadName = sceneToLoadName;
			
			if (antiSpillSceneName == "")
			{
				_antiSpillScene = SceneManager.CreateScene($"AntiSpill_{sceneToLoadName}");

				PrepareAntiFillSetSceneActive();
			}
			else
			{
				_scenesInBuild = MMScene.GetScenesInBuild();
				if (!_scenesInBuild.Contains(antiSpillSceneName))
				{
					Debug.LogError("MMSceneLoadingAntiSpill : impossible to load the '"+antiSpillSceneName+"' scene, " +
					               "there is no such scene in the project's build settings.");
					return;
				}
				
				SceneManager.LoadScene(antiSpillSceneName, LoadSceneMode.Additive);
				_antiSpillScene = SceneManager.GetSceneByName(antiSpillSceneName);
				_antiSpillSceneName = _antiSpillScene.name;
				SceneManager.sceneLoaded += PrepareAntiFillOnSceneLoaded;
			}
		}

		/// <summary>
		/// When not creating an anti fill scene, acts once the scene has been actually created and is ready to be set active
		/// This is bypassed when creating the scene
		/// </summary>
		/// <param name="newScene"></param>
		/// <param name="mode"></param>
		protected virtual void PrepareAntiFillOnSceneLoaded(Scene newScene, LoadSceneMode mode)
		{
			if (newScene.name != _antiSpillSceneName)
			{
				return;
			}
			SceneManager.sceneLoaded -= PrepareAntiFillOnSceneLoaded;
			PrepareAntiFillSetSceneActive();
		}

		/// <summary>
		/// Sets the anti spill scene active
		/// </summary>
		protected virtual void PrepareAntiFillSetSceneActive()
		{
			if (_onActiveSceneChangedCallback != null) { SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback; }
			_onActiveSceneChangedCallback = OnActiveSceneChanged;
			SceneManager.activeSceneChanged += _onActiveSceneChangedCallback;
			SceneManager.SetActiveScene(_antiSpillScene);
		}
		
		/// <summary>
		/// Once the destination scene has been loaded, we catch that event and prepare to empty
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		protected virtual void OnActiveSceneChanged(Scene from, Scene to)
		{
			if (from == _antiSpillScene)
			{
				SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback;
				_onActiveSceneChangedCallback = null;
				
				EmptyAntiSpillScene();
			}
		}

		/// <summary>
		/// Empties the contents of the anti spill scene into the destination scene
		/// </summary>
		protected virtual void EmptyAntiSpillScene()
		{
			if (_antiSpillScene.IsValid() && _antiSpillScene.isLoaded)
			{
				_spillSceneRoots.Clear();
				_antiSpillScene.GetRootGameObjects(_spillSceneRoots);

				_destinationScene = SceneManager.GetSceneByName(_sceneToLoadName);
				
				if (_spillSceneRoots.Count > 0)
				{
					if (_destinationScene.IsValid() && _destinationScene.isLoaded)
					{
						foreach (var root in _spillSceneRoots)
						{
							SceneManager.MoveGameObjectToScene(root, _destinationScene);
						}
					}
				}

				SceneManager.UnloadSceneAsync(_antiSpillScene);
			}
		}
	}
}