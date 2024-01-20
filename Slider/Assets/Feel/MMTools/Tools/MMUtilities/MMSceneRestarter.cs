using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
	/// <summary>
	/// This component lets you restart a scene by pressing a key
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Utilities/MMSceneRestarter")]
	public class MMSceneRestarter : MonoBehaviour
	{
		/// the possible restart modes
		public enum RestartModes { ActiveScene, SpecificScene }

		[Header("Settings")]
		/// the selected restart mode, either the currently active scene, or one by name
		public RestartModes RestartMode = RestartModes.ActiveScene;
		/// the name of the scene to load if we're in specific scene mode
		[MMEnumCondition("RestartMode", (int)RestartModes.SpecificScene)]
		public string SceneName;
		/// the load mode
		public LoadSceneMode LoadMode = LoadSceneMode.Single;

		[Header("Input")]
        
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			/// the key to press to restart manually
			public Key RestarterKey = Key.Backspace;
		#else
		/// the key to press to restart manually
		public KeyCode RestarterKeyCode = KeyCode.Backspace;
		#endif

		protected string _newSceneName;

		/// <summary>
		/// On Update, looks for input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// Looks for a key press of the specified key
		/// </summary>
		protected virtual void HandleInput()
		{
			bool keyPressed = false;
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				keyPressed = Keyboard.current[RestarterKey].wasPressedThisFrame;
			#else
			keyPressed = Input.GetKeyDown(RestarterKeyCode);
			#endif
			if (keyPressed)
			{
				RestartScene();
			}
		}

		/// <summary>
		/// Restarts the scene based on the specified settings
		/// </summary>
		public virtual void RestartScene()
		{
			Debug.Log("Scene restarted by MMSceneRestarter");
			switch (RestartMode)
			{
				case RestartModes.ActiveScene:
					Scene scene = SceneManager.GetActiveScene();
					_newSceneName = scene.name;
					break;

				case RestartModes.SpecificScene:
					_newSceneName = SceneName;
					break;
			}
			SceneManager.LoadScene(_newSceneName, LoadMode);
		}
	}
}