using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control a camera's orthographic size over time. You'll need a MMCameraOrthographicSizeShaker on your camera.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Camera/Orthographic Size")]
	[FeedbackHelp("This feedback lets you control a camera's orthographic size over time. You'll need a MMCameraOrthographicSizeShaker on your camera.")]
	public class MMF_CameraOrthographicSize : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;

		[MMFInspectorGroup("Orthographic Size", true, 41)]
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
		public bool RelativeOrthographicSize = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeOrthographicSize = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapOrthographicSizeZero = 5f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapOrthographicSizeOne = 10f;

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
			MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration, RemapOrthographicSizeZero, RemapOrthographicSizeOne, RelativeOrthographicSize,
				feedbacksIntensity, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
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
			MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration,
				RemapOrthographicSizeZero, RemapOrthographicSizeOne, stop: true);
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
			MMCameraOrthographicSizeShakeEvent.Trigger(ShakeOrthographicSize, FeedbackDuration,
				RemapOrthographicSizeZero, RemapOrthographicSizeOne, restore: true);
		}
	}
}