using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to store MMInputExecution bindings, associating a target keycode to UnityEvents
	/// </summary>
	[System.Serializable]
	public class MMInputExecutionBinding
	{
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			public Key TargetInputKey = Key.Space;
		#else
		/// the key the user needs to press to trigger events
		public KeyCode TargetKey = KeyCode.Space;
		#endif

		/// the event to trigger when the key is pressed down
		public UnityEvent OnKeyDown;
		/// the event to trigger every frame if the key is being pressed 
		public UnityEvent OnKey;
		/// the event to trigger when the key is released
		public UnityEvent OnKeyUp;

		/// <summary>
		/// Checks for input and invokes events if needed
		/// </summary>
		public virtual void ProcessInput()
		{
			bool key = false;
			bool keyDown = false;
			bool keyUp = false;
	        
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		        key = Keyboard.current[TargetInputKey].isPressed;
		        keyDown = Keyboard.current[TargetInputKey].wasPressedThisFrame;
		        keyUp = Keyboard.current[TargetInputKey].wasReleasedThisFrame;
			#else
			key = Input.GetKey(TargetKey);
			keyDown = Input.GetKeyDown(TargetKey);
			keyUp = Input.GetKeyUp(TargetKey);
			#endif
	        
			if (OnKey != null)
			{
				if (key)
				{
					OnKey.Invoke();
				}
			}
			if (OnKeyDown != null)
			{
				if (keyDown)
				{
					OnKeyDown.Invoke();
				}
			}
			if (OnKeyUp != null)
			{
				if (keyUp)
				{
					OnKeyUp.Invoke();
				}
			}
		}
	}

	/// <summary>
	/// A simple class used to bind target keys to specific events to trigger when the key is pressed or released
	/// </summary>
	public class MMInputExecution : MonoBehaviour
	{
		[Header("Bindings")]
		/// a list of bindings
		public List<MMInputExecutionBinding> Bindings;
        
		/// <summary>
		/// On update we process our input
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// Parses all bindings and asks them to trigger events if needed
		/// </summary>
		protected virtual void HandleInput()
		{
			if (Bindings == null)
			{
				return;
			}
			foreach(MMInputExecutionBinding binding in Bindings)
			{
				binding.ProcessInput();
			}            
		}
	}
}