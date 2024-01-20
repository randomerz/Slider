using UnityEngine;
using MoreMountains.Feedbacks;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this feedback to interact with haptics at a global level, stopping them all, enabling or disabling them, adjusting their global level or initializing/release the haptic engine
	/// </summary>
	[AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Control")]
	#endif
	[FeedbackHelp("Add this feedback to interact with haptics at a global level, stopping them all, enabling or disabling them, adjusting their global level or initializing/release the haptic engine.")]
	public class MMFeedbackNVControl : MMFeedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		#endif
    
		public enum ControlTypes { Stop, EnableHaptics, DisableHaptics, AdjustHapticsLevel, Initialize, Release }

		[Header("Haptic Control")]
		/// the type of control order to trigger when playing this feedback - check Nice Vibrations' documentation for the exact behaviour of these 
		[Tooltip("the type of control order to trigger when playing this feedback - check Nice Vibrations' documentation for the exact behaviour of these")]
		public ControlTypes ControlType = ControlTypes.Stop;
		/// the output level when in AdjustHapticsLevel mode
		[Tooltip("the output level when in AdjustHapticsLevel mode")]
		[MMFEnumCondition("ControlType", (int)ControlTypes.AdjustHapticsLevel)]
		public float OutputLevel = 1f;
        
		/// <summary>
		/// On play we apply the specified order
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (ControlType)
			{
				case ControlTypes.Stop:
					HapticController.Stop();
					break;
				case ControlTypes.EnableHaptics:
					HapticController.hapticsEnabled = true;
					break;
				case ControlTypes.DisableHaptics:
					HapticController.hapticsEnabled = false;
					break;
				case ControlTypes.AdjustHapticsLevel:
					HapticController.outputLevel = OutputLevel;
					break;
				case ControlTypes.Initialize:
					LofeltHaptics.Initialize();
					HapticController.Init();
					break;
				case ControlTypes.Release:
					LofeltHaptics.Release();
					break;
			}
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}