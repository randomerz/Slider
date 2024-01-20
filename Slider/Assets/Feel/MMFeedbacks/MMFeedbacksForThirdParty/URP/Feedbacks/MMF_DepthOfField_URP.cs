using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control URP depth of field focus distance, aperture and focal length over time. 
	/// It requires you have in your scene an object with a Volume 
	/// with Depth of Field active, and a MMDepthOfFieldShaker_URP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to control URP depth of field focus distance, aperture and focal length over time. " +
	              "It requires you have in your scene an object with a Volume " +
	              "with Depth of Field active, and a MMDepthOfFieldShaker_URP component.")]
	#if MM_URP
	[FeedbackPath("PostProcess/Depth Of Field URP")]
	#endif
	public class MMF_DepthOfField_URP : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Depth Of Field", true, 49)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float ShakeDuration = 2f;
		/// whether or not to add to the initial values
		[Tooltip("whether or not to add to the initial values")]
		public bool RelativeValues = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Focus Distance", true, 50)]
		/// the curve used to animate the focus distance value on
		[Tooltip("the curve used to animate the focus distance value on")]
		public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapFocusDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapFocusDistanceOne = 3f;

		[MMFInspectorGroup("Aperture", true, 51)]
		/// the curve used to animate the aperture value on
		[Tooltip("the curve used to animate the aperture value on")]
		public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0.1f, 32f)]
		public float RemapApertureZero = .1f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0.1f, 32f)]
		public float RemapApertureOne = 32f;

		[MMFInspectorGroup("Focal Length", true, 20)]
		/// the curve used to animate the focal length value on
		[Tooltip("the curve used to animate the focal length value on")]
		public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 300f)]
		public float RemapFocalLengthZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 300f)]
		public float RemapFocalLengthOne = 0f;

		/// <summary>
		/// Triggers a depth of field event
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
			MMDepthOfFieldShakeEvent_URP.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
				ShakeAperture, RemapApertureZero, RemapApertureOne,
				ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
				RelativeValues, intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
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
            
			MMDepthOfFieldShakeEvent_URP.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
				ShakeAperture, RemapApertureZero, RemapApertureOne,
				ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
				RelativeValues, channelData: ChannelData, stop: true );
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
            
			MMDepthOfFieldShakeEvent_URP.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
				ShakeAperture, RemapApertureZero, RemapApertureOne,
				ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
				RelativeValues, channelData: ChannelData, restore: true );
		}
	}
}