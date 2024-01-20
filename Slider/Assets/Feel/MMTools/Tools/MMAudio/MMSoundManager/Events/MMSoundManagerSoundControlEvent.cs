using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerSoundControlEventTypes
	{
		Pause,
		Resume,
		Stop,
		Free
	}
    
	/// <summary>
	/// An event used to control a specific sound on the MMSoundManager.
	/// You can either search for it by ID, or directly pass an audiosource if you have it.
	///
	/// Example : MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, 33);
	/// will cause the sound(s) with an ID of 33 to stop playing
	/// </summary>
	public struct MMSoundManagerSoundControlEvent
	{
		/// the ID of the sound to control (has to match the one used to play it)
		public int SoundID;
		/// the control mode
		public MMSoundManagerSoundControlEventTypes MMSoundManagerSoundControlEventType;
		/// the audiosource to control (if specified)
		public AudioSource TargetSource;
        
		public MMSoundManagerSoundControlEvent(MMSoundManagerSoundControlEventTypes eventType, int soundID, AudioSource source = null)
		{
			SoundID = soundID;
			TargetSource = source;
			MMSoundManagerSoundControlEventType = eventType;
		}

		static MMSoundManagerSoundControlEvent e;
		public static void Trigger(MMSoundManagerSoundControlEventTypes eventType, int soundID, AudioSource source = null)
		{
			e.SoundID = soundID;
			e.TargetSource = source;
			e.MMSoundManagerSoundControlEventType = eventType;
			MMEventManager.TriggerEvent(e);
		}
	}
}