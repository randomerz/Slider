using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A super simple mono you can add to an object to call its Quit method, which will force the application to quit.
	/// </summary>
	public class MMApplicationQuit : MonoBehaviour
	{
		[Header("Debug")]
		[MMInspectorButton("Quit")] 
		public bool QuitButton;
        
		/// <summary>
		/// Forces the application to quit
		/// </summary>
		public virtual void Quit()
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
                Application.Quit();
			#endif
		}
	}
}