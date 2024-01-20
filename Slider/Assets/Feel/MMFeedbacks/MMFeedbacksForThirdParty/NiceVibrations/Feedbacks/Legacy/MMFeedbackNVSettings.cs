using UnityEngine;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// A class used to store and manage common Nice Vibrations feedback settings
	/// </summary>
	[System.Serializable]
	public class MMFeedbackNVSettings
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// whether or not to force this haptic to play on a specific gamepad
		[Tooltip("whether or not to force this haptic to play on a specific gamepad")]
		public bool ForceGamepadID = false;
		/// The ID of the gamepad on which to play this haptic
		[Tooltip("The ID of the gamepad on which to play this haptic")]
		public int GamepadID = 0;
		/// whether or not this haptic should play only if haptics are supported
		[Tooltip("whether or not this haptic should play only if haptics are supported")]
		public bool OnlyPlayIfHapticsSupported = true;
		/// whether or not this haptic should play only if advanced haptics requirements are met on the device
		[Tooltip("whether or not this haptic should play only if advanced haptics requirements are met on the device")]
		public bool OnlyPlayIfAdvancedRequirementsMet = false;
		/// whether or not this haptic should play only if the device supports amplitude modulation
		[Tooltip("whether or not this haptic should play only if the device supports amplitude modulation")]
		public bool OnlyPlayIfAmplitudeModulationSupported = false;
		/// whether or not this haptic should play only if the device supports frequency modulation
		[Tooltip("whether or not this haptic should play only if the device supports frequency modulation")]
		public bool OnlyPlayIfFrequencyModulationSupported = false;

		/// <summary>
		/// If necessary, forces the current haptic to play on a specific gamepad
		/// </summary>
		public virtual void SetGamepad()
		{
			if (ForceGamepadID)
			{
				GamepadRumbler.SetCurrentGamepad(GamepadID);
			}
		}
        
		/// <summary>
		/// Whether or not this haptic can play based on the specified conditions
		/// </summary>
		/// <returns></returns>
		public virtual bool CanPlay()
		{
			#if UNITY_IOS || UNITY_ANDROID
            if (OnlyPlayIfHapticsSupported && !DeviceCapabilities.isVersionSupported)
            {
                return false;
            }
			#endif

			if (OnlyPlayIfAdvancedRequirementsMet && !DeviceCapabilities.meetsAdvancedRequirements)
			{
				return false;
			}

			if (OnlyPlayIfAmplitudeModulationSupported && !DeviceCapabilities.hasAmplitudeModulation)
			{
				return false;
			}

			if (OnlyPlayIfFrequencyModulationSupported && !DeviceCapabilities.hasFrequencyModulation)
			{
				return false;
			}

			return true;
		}
		#endif
	}
}