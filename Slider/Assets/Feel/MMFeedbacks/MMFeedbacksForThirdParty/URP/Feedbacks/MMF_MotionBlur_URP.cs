using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control URP motion blur intensity over time.
	/// It requires you have in your scene an object with a Volume 
	/// with MotionBlur active, and a MMMotionBlurShaker_URP component.
	/// </summary>
	[AddComponentMenu("")]
	#if MM_URP
	[FeedbackPath("PostProcess/Motion Blur URP")]
	#endif
	[FeedbackHelp("This feedback allows you to control motion blur intensity over time. " +
	              "It requires you have in your scene an object with a Volume " +
	              "with MotionBlur active, and a MMMotionBlurShaker_URP component.")]
	public class MMF_MotionBlur_URP : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Motion Blur", true, 25)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Intensity", true, 24)]
		/// the curve to animate the intensity on
		[Tooltip("the curve to animate the intensity on")]
		public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to which to remap the curve's zero to
		[Tooltip("the value to which to remap the curve's zero to")]
		[Range(0f, 1f)]
		public float RemapIntensityZero = 0f;
		/// the value to which to remap the curve's one to
		[Tooltip("the value to which to remap the curve's one to")]
		[Range(0f, 1f)]
		public float RemapIntensityOne = 1.0f;
		/// whether or not to add to the initial intensity
		[Tooltip("whether or not to add to the initial intensity")]
		public bool RelativeIntensity = false;

		/// <summary>
		/// Triggers a motion blur shake
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMMotionBlurShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
				ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
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
			MMMotionBlurShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, stop: true, channelData: ChannelData);
		}

		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			MMMotionBlurShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, restore: true, channelData: ChannelData);
		}
	}
}