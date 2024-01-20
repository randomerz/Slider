using UnityEngine;
using MoreMountains.Feedbacks;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Use this feedback to play an Emphasis haptics, short haptic bursts whose amplitude and frequency can be controlled in real time, also called Transients in CoreHaptics/iOS
	/// </summary>
	[AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Emphasis")]
	#endif 
	[FeedbackHelp("Use this feedback to play an Emphasis haptics, short haptic bursts whose amplitude and frequency can be controlled in real time, also called Transients in CoreHaptics/iOS")]
	public class MMF_NVEmphasis : MMF_Feedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		#endif
        
		[MMFInspectorGroup("Haptic Amplitude", true, 23)]
		/// the minimum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)
		[Tooltip("the minimum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)")]
		[Range(0f, 1f)]
		public float MinAmplitude = 1f;
		/// the maximum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)
		[Tooltip("the maximum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)")]
		[Range(0f, 1f)]
		public float MaxAmplitude = 1f;
        
		[MMFInspectorGroup("Haptic Frequency", true, 22)]
		/// the minimum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)
		[Tooltip("the minimum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)")]
		[Range(0f, 1f)]
		public float MinFrequency = 1f;
		/// the maximum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)
		[Tooltip("the maximum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)")]
		[Range(0f, 1f)]
		public float MaxFrequency = 1f;

		[MMFInspectorGroup("Settings", true, 16)]
		/// a set of settings you can tweak to specify how and when exactly this haptic should play
		[Tooltip("a set of settings you can tweak to specify how and when exactly this haptic should play")]
		public MMFeedbackNVSettings HapticSettings;
        
		/// <summary>
		/// On play, we randomize our amplitude and frequency and play our emphasis haptic
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !HapticSettings.CanPlay())
			{
				return;
			}

			float amplitude = Random.Range(MinAmplitude, MaxAmplitude);
			float frequency = Random.Range(MinFrequency, MaxFrequency);
			HapticSettings.SetGamepad();
			HapticPatterns.PlayEmphasis(amplitude, frequency);
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}