using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerEventTypes
	{
		SaveSettings,
		LoadSettings,
		ResetSettings,
		SettingsLoaded
	}
    
	/// <summary>
	/// This event will let you trigger a save/load/reset on the MMSoundManager settings
	///
	/// Example : MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.SaveSettings);
	/// will save settings. 
	/// </summary>
	public struct MMSoundManagerEvent
	{
		public MMSoundManagerEventTypes EventType;
        
		public MMSoundManagerEvent(MMSoundManagerEventTypes eventType)
		{
			EventType = eventType;
		}

		static MMSoundManagerEvent e;
		public static void Trigger(MMSoundManagerEventTypes eventType)
		{
			e.EventType = eventType;
			MMEventManager.TriggerEvent(e);
		}
	}
}