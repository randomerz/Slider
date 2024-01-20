using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control bloom intensity and threshold over time. It requires you have in your scene an object with a Volume with Bloom active, and a MMBloomShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to control bloom intensity and threshold over time. It requires you have in your scene an object with a Volume " +
	              "with Bloom active, and a MMBloomShaker_HDRP component.")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Bloom HDRP")]
	#endif
	public class MMF_Bloom_HDRP : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); }  set { ShakeDuration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Bloom", true, 3)]
		/// the duration of the feedback, in seconds
		[Tooltip("the duration of the feedback, in seconds")]
		public float ShakeDuration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial intensity
		[Tooltip("whether or not to add to the initial intensity")]
		public bool RelativeValues = true;

		[MMFInspectorGroup("Intensity", true, 4)]
		/// the curve to animate the intensity on
		[Tooltip("the curve to animate the intensity on")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapIntensityOne = 1f;

		[MMFInspectorGroup("Threshold", true, 5)]
		/// the curve to animate the threshold on
		[Tooltip("the curve to animate the threshold on")]
		public AnimationCurve ShakeThreshold = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapThresholdZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapThresholdOne = 0f;

		/// <summary>
		/// Triggers a bloom shake
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			attenuation = ComputeIntensity(attenuation, position);
            
			MMBloomShakeEvent_HDRP.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
				RelativeValues, attenuation, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
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
            
			MMBloomShakeEvent_HDRP.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
				RelativeValues, channelData:ChannelData, stop:true);
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
            
			MMBloomShakeEvent_HDRP.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
				RelativeValues, channelData:ChannelData, restore:true);
		}
	}
}