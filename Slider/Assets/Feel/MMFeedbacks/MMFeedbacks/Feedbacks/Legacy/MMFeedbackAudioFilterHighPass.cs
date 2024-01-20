using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the cutoff frequency of a high pass filter. You'll need a MMAudioFilterHighPassShaker on your filter.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/Audio Filter High Pass")]
	[FeedbackHelp("This feedback lets you control a high pass audio filter over time. You'll need a MMAudioFilterHighPassShaker on your filter.")]
	public class MMFeedbackAudioFilterHighPass : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		[Header("High Pass Feedback")]
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[Header("High Pass")]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeHighPass = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeHighPass = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Range(10f, 22000f)]
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapHighPassZero = 0f;
		/// the value to remap the curve's 1 to
		[Range(10f, 22000f)]
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapHighPassOne = 10000f;

		/// <summary>
		/// Triggers the corresponding coroutine
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMAudioFilterHighPassShakeEvent.Trigger(ShakeHighPass, FeedbackDuration, RemapHighPassZero, RemapHighPassOne, RelativeHighPass,
				intensityMultiplier, ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMAudioFilterHighPassShakeEvent.Trigger(ShakeHighPass, FeedbackDuration, RemapHighPassZero, RemapHighPassOne, stop:true);
		}
	}
}