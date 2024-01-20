using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control Panini Projection distance and crop to fit over time. 
	/// It requires you have in your scene an object with a Volume with Bloom active, and a MMPaniniProjectionShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to control Panini Projection distance and crop to fit over time. " +
	              "It requires you have in your scene an object with a Volume " +
	              "with PaniniProjection active, and a MMPaniniProjectionShaker_HDRP component.")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Panini Projection HDRP")]
	#endif
	public class MMF_PaniniProjection_HDRP : MMF_Feedback
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

		[MMFInspectorGroup("Panini Projection", true, 26)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Distance", true, 27)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeDistance = false;
		/// the curve used to animate the distance value on
		[Tooltip("the curve used to animate the distance value on")]
		public AnimationCurve ShakeDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapDistanceOne = 1f;

		/// <summary>
		/// Triggers a bloom shake
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
			MMPaniniProjectionShakeEvent_HDRP.Trigger(ShakeDistance, FeedbackDuration, RemapDistanceZero, RemapDistanceOne, RelativeDistance, intensityMultiplier, ChannelData, 
				ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
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
			MMPaniniProjectionShakeEvent_HDRP.Trigger(ShakeDistance, FeedbackDuration, RemapDistanceZero, RemapDistanceOne, RelativeDistance, channelData:ChannelData, stop:true);
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
			
			MMPaniniProjectionShakeEvent_HDRP.Trigger(ShakeDistance, FeedbackDuration, RemapDistanceZero, RemapDistanceOne, RelativeDistance, channelData:ChannelData, restore:true);
		}
	}
}