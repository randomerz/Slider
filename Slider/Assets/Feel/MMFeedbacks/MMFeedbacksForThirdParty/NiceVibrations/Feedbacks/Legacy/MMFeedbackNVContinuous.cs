using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this feedback to play a continuous haptic of the specified amplitude and frequency over a certain duration. This feedback will also let you randomize these, and modulate them over time.
	/// </summary>
	[AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Continuous")]
	#endif
	[FeedbackHelp("Add this feedback to play a continuous haptic of the specified amplitude and frequency over a certain duration. This feedback will also let you randomize these, and modulate them over time.")]
	public class MMFeedbackNVContinuous : MMFeedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		#endif
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(_duration); } set { _duration = value; } }
        
		[Header("Haptic Amplitude")]
		/// the minimum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)
		[Tooltip("the minimum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)")]
		[Range(0f, 1f)]
		public float MinAmplitude = 1f;
		/// the maximum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)
		[Tooltip("the maximum amplitude at which this clip should play (amplitude will be randomized between MinAmplitude and MaxAmplitude)")]
		[Range(0f, 1f)]
		public float MaxAmplitude = 1f;
        
		[Header("Haptic Frequency")]
		/// the minimum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)
		[Tooltip("the minimum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)")]
		[Range(0f, 1f)]
		public float MinFrequency = 1f;
		/// the maximum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)
		[Tooltip("the maximum frequency at which this clip should play (frequency will be randomized between MinFrequency and MaxFrequency)")]
		[Range(0f, 1f)]
		public float MaxFrequency = 1f;
        
		[Header("Duration")]
		/// the minimum duration at which this clip should play (duration will be randomized between MinDuration and MaxDuration)
		[Tooltip("the minimum duration at which this clip should play (duration will be randomized between MinDuration and MaxDuration)")]
		public float MinDuration = 1f;
		/// the maximum duration at which this clip should play (duration will be randomized between MinDuration and MaxDuration)
		[Tooltip("the maximum duration at which this clip should play (duration will be randomized between MinDuration and MaxDuration)")]
		public float MaxDuration = 1f;


		[Header("Real-time Modulation")] 
		/// whether or not to modulate the haptic signal at runtime
		[Tooltip("whether or not to modulate the haptic signal at runtime")]
		public bool UseRealTimeModulation = false;
		/// if UseRealTimeModulation:true, the curve along which to modulate amplitude for this continuous haptic, over its total duration
		[Tooltip("if UseRealTimeModulation:true, the curve along which to modulate amplitude for this continuous haptic, over its total duration")]
		[MMFCondition("UseRealTimeModulation", true)]
		public AnimationCurve AmplitudeMultiplication = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// if UseRealTimeModulation:true, the curve along which to modulate frequency for this continuous haptic, over its total duration
		[Tooltip("if UseRealTimeModulation:true, the curve along which to modulate frequency for this continuous haptic, over its total duration")]
		[MMFCondition("UseRealTimeModulation", true)]
		public AnimationCurve ShiftFrequency = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));

		[Header("Settings")] 
		/// a set of settings you can tweak to specify how and when exactly this haptic should play
		[Tooltip("a set of settings you can tweak to specify how and when exactly this haptic should play")]
		public MMFeedbackNVSettings HapticSettings;
        
		protected Coroutine _coroutine;
		protected float _duration = 0f;
        
		/// <summary>
		/// On play we randomize our amplitude and frequency, trigger our haptic, and initialize real time modulation if needed
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
			_duration = Random.Range(MinDuration, MaxDuration);
			HapticSettings.SetGamepad();
			HapticPatterns.PlayConstant(amplitude, frequency, FeedbackDuration);

			if (UseRealTimeModulation)
			{
				_coroutine = StartCoroutine(RealtimeModulationCo());
			}
		}
        
		/// <summary>
		/// A coroutine used to modulate frequency and amplitude at runtime
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RealtimeModulationCo()
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				HapticController.clipLevel = AmplitudeMultiplication.Evaluate(remappedTime);
				HapticController.clipFrequencyShift = ShiftFrequency.Evaluate(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			HapticController.clipLevel = AmplitudeMultiplication.Evaluate(FinalNormalizedTime);
			HapticController.clipFrequencyShift = ShiftFrequency.Evaluate(FinalNormalizedTime);       
            
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}
        
		/// <summary>
		/// On stop we stop haptics and our coroutine
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!FeedbackTypeAuthorized)
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			HapticController.Stop();
			if (Active && (_coroutine != null))
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}