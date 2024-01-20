using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{
	public class MMAudioEvents 
	{
        
	}
    
	/// <summary>
	/// A struct used to trigger sounds
	/// </summary>
	public struct MMSfxEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AudioClip clipToPlay, AudioMixerGroup audioGroup = null, float volume = 1f, float pitch = 1f, int priority = 128);
		static public void Trigger(AudioClip clipToPlay, AudioMixerGroup audioGroup = null, float volume = 1f, float pitch = 1f, int priority = 128)
		{
			OnEvent?.Invoke(clipToPlay, audioGroup, volume, pitch, priority);
		}
	}
}