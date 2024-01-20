using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the stereo pan of a target AudioSource over time.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/AudioSource Stereo Pan")]
	[FeedbackHelp("This feedback lets you control the stereo pan of a target AudioSource over time.")]
	public class MMF_AudioSourceStereoPan : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("AudioSource Stereo Pan", true, 48)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeStereoPan = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeStereoPan = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Range(-1f, 1f)]
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapStereoPanZero = 0f;
		/// the value to remap the curve's 1 to
		[Range(-1f, 1f)]
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapStereoPanOne = 1f;


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
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, RelativeStereoPan,
				intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
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
			MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, stop:true);
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, restore:true);
		}
	}
}