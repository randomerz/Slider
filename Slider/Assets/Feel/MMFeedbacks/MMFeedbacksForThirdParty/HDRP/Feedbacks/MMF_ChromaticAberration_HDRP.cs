using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control chromatic aberration intensity over time. It requires you have in your scene an object with a Volume 
	/// with HDRP Chromatic Aberration active, and a MMChromaticAberrationShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Chromatic Aberration HDRP")]
	#endif
	[FeedbackHelp("This feedback allows you to control chromatic aberration intensity over time. It requires you have in your scene an object with a Volume " +
	              "with Chromatic Aberration active, and a MMChromaticAberrationShaker_HDRP component.")]
	public class MMF_ChromaticAberration_HDRP : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Chromatic Aberration", true, 10)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapIntensityOne = 1f;

		[MMFInspectorGroup("Intensity", true, 11)]
		/// the curve to animate the intensity on
		[Tooltip("the curve to animate the intensity on")]
		public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the multiplier to apply to the intensity curve
		[Tooltip("the multiplier to apply to the intensity curve")]
		[Range(0f, 1f)]
		public float Amplitude = 1.0f;
		/// whether or not to add to the initial intensity
		[Tooltip("whether or not to add to the initial intensity")]
		public bool RelativeIntensity = false;

		/// <summary>
		/// Triggers a chromatic aberration shake
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
			MMChromaticAberrationShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
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
            
			MMChromaticAberrationShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, channelData:ChannelData, stop:true);
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
			
			MMChromaticAberrationShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, channelData:ChannelData, restore:true);
		}
	}
}