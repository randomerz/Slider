using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the wetmix level of an echo filter. You'll need a MMAudioFilterEchoShaker on your filter.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/Audio Filter Echo")]
	[FeedbackHelp("This feedback lets you control an echo audio filter's wet mix value over time. You'll need a MMAudioFilterEchoShaker on your filter.")]
	public class MMFeedbackAudioFilterEcho : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		[Header("Echo Feedback")]
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

		[Header("Echo")]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeEcho = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeEcho = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Range(0f, 1f)]
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapEchoZero = 0f;
		/// the value to remap the curve's 1 to
		[Range(0f, 1f)]
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapEchoOne = 1f;

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
			MMAudioFilterEchoShakeEvent.Trigger(ShakeEcho, FeedbackDuration, RemapEchoZero, RemapEchoOne, RelativeEcho,
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
			MMAudioFilterEchoShakeEvent.Trigger(ShakeEcho, FeedbackDuration, RemapEchoZero, RemapEchoOne, RelativeEcho, stop:true);
		}
	}
}