using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

namespace MoreMountains.Tools
{
	/// <summary>
	/// This helper class handles adding the appropriate input module depending on whether the project is using the old or new input system
	/// </summary>
	public class MMAutoInputModule : MonoBehaviour
	{
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		protected InputSystemUIInputModule _module;
		#endif

		protected GameObject _eventSystemGameObject;
		
		/// <summary>
		/// On Awake, we initialize the input module
		/// </summary>
		protected virtual void Awake()
		{
			StartCoroutine(InitializeInputModule());
		}

		/// <summary>
		/// We add the appropriate input module
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator InitializeInputModule()
		{
			EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();

			if (eventSystem == null)
			{
				yield break;
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				_eventSystemGameObject = eventSystem.gameObject;
				_module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
				// thanks new input system.
				yield return null;
				_module.enabled = false;
				yield return null;
				_module.enabled = true;
			#else
			eventSystem.gameObject.AddComponent<StandaloneInputModule>();
			#endif
			yield return null;
		}
	}	
}