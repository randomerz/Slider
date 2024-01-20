using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the distortion level of a distortion filter. You'll need a MMAudioFilterDistortionShaker on the filter.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio/Audio Filter Distortion")]
	[FeedbackHelp("This feedback lets you control a distortion audio filter over time. You'll need a MMAudioFilterDistortionShaker on the filter.")]
	public class MMFeedbackAudioFilterDistortion : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		[Header("Distortion Feedback")]
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

		[Header("Distortion")]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeDistortion = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapDistortionZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapDistortionOne = 1f;

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
			float remapZero = 0f;
			float remapOne = 0f;
            
			if (!Timing.ConstantIntensity)
			{
				remapZero = RemapDistortionZero * intensityMultiplier;
				remapOne = RemapDistortionOne * intensityMultiplier;
			}
            
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, remapZero, remapOne, RelativeDistortion,
				intensityMultiplier, ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, 0f,0f, stop:true);
		}
	}
}