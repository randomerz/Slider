using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control white balance temperature and tint over time. 
	/// It requires you have in your scene an object with a Volume with White Balance active, and a MMWhiteBalanceShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to control white balance temperature and tint over time. " +
	              "It requires you have in your scene an object with a Volume " +
	              "with WhiteBalance active, and a MMWhiteBalanceShaker_HDRP component.")]
	[FeedbackPath("PostProcess/White Balance HDRP")]
	public class MMFeedbackWhiteBalance_HDRP : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		[Header("White Balance")]
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float ShakeDuration = 1f;
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeValues = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[Header("Temperature")]
		/// the curve used to animate the temperature value on
		[Tooltip("the curve used to animate the temperature value on")]
		public AnimationCurve ShakeTemperature = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapTemperatureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapTemperatureOne = 100f;

		[Header("Tint")]
		/// the curve used to animate the tint value on
		[Tooltip("the curve used to animate the tint value on")]
		public AnimationCurve ShakeTint = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapTintZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapTintOne = 100f;

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value; } }

		/// <summary>
		/// Triggers a white balance shake
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMWhiteBalanceShakeEvent_HDRP.Trigger(ShakeTemperature, FeedbackDuration, RemapTemperatureZero, RemapTemperatureOne,
				ShakeTint, RemapTintZero, RemapTintOne, RelativeValues, intensityMultiplier,
				ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
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
			MMWhiteBalanceShakeEvent_HDRP.Trigger(ShakeTemperature, FeedbackDuration, RemapTemperatureZero, RemapTemperatureOne,
				ShakeTint, RemapTintZero, RemapTintOne, RelativeValues, channelData:ChannelData(Channel), stop:true);
		}
	}
}