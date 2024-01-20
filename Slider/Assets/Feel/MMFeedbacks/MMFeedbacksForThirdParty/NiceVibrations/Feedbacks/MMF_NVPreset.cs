using UnityEngine;
using MoreMountains.Feedbacks;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Use this feedback to play a preset haptic, limited but super simple predifined haptic patterns
	/// </summary>
	[AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Preset")]
	#endif    
	[FeedbackHelp("Use this feedback to play a preset haptic, limited but super simple predifined haptic patterns")]
	public class MMF_NVPreset : MMF_Feedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		public override string RequiredTargetText { get { return Preset.ToString();  } }
		#endif
    
		[MMFInspectorGroup("Haptic Preset", true, 21)]
		/// the preset to play with this feedback
		[Tooltip("the preset to play with this feedback")]
		public HapticPatterns.PresetType Preset = HapticPatterns.PresetType.LightImpact;

		[MMFInspectorGroup("Settings", true, 16)]
		/// a set of settings you can tweak to specify how and when exactly this haptic should play
		[Tooltip("a set of settings you can tweak to specify how and when exactly this haptic should play")]
		public MMFeedbackNVSettings HapticSettings;
        
		/// <summary>
		/// On play we play our preset haptic
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !HapticSettings.CanPlay())
			{
				return;
			}

			HapticSettings.SetGamepad();
			HapticPatterns.PlayPreset(Preset);
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}