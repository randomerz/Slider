using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple component that will ensure (if put on ALL audio listeners in your game)
	/// that you never see a "There are two audio listeners in the scene" warning again.
	/// </summary>
	[RequireComponent(typeof(AudioListener))]
	public class MMAudioListener : MonoBehaviour
	{
		protected AudioListener _audioListener;
		protected AudioListener[] _otherListeners;
        
		/// <summary>
		/// On enable, disables other listeners if found
		/// </summary>
		protected virtual void OnEnable()
		{
			_audioListener = this.gameObject.GetComponent<AudioListener>();
			_otherListeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];

			foreach (AudioListener audioListener in _otherListeners)
			{
				if ((audioListener != null) && (audioListener != _audioListener) )
				{
					audioListener.enabled = false;
				}    
			}
		}
	}    
}