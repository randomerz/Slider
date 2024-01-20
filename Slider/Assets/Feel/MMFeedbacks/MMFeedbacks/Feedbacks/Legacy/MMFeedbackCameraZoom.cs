using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback that will allow you to change the zoom of a (3D) camera when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("Define zoom properties : For will set the zoom to the specified parameters for a certain duration, " +
	              "Set will leave them like that forever. Zoom properties include the field of view, the duration of the " +
	              "zoom transition (in seconds) and the zoom duration (the time the camera should remain zoomed in, in seconds). " +
	              "For this to work, you'll need to add a MMCameraZoom component to your Camera, or a MMCinemachineZoom if you're " +
	              "using virtual cameras.")]
	[FeedbackPath("Camera/Camera Zoom")]
	public class MMFeedbackCameraZoom : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		[Header("Camera Zoom")]
		/// the channel to broadcast that zoom event on
		[Tooltip("the channel to broadcast that zoom event on")]
		public int Channel = 0;
		/// the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)
		[Tooltip("the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)")]
		public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
		/// the target field of view
		[Tooltip("the target field of view")]
		public float ZoomFieldOfView = 30f;
		/// the zoom transition duration
		[Tooltip("the zoom transition duration")]
		public float ZoomTransitionDuration = 0.05f;
		/// the duration for which the zoom is at max zoom
		[Tooltip("the duration for which the zoom is at max zoom")]
		public float ZoomDuration = 0.1f;
		/// whether or not ZoomFieldOfView should add itself to the current camera's field of view value
		[Tooltip("whether or not ZoomFieldOfView should add itself to the current camera's field of view value")]
		public bool RelativeFieldOfView = false;

		/// the duration of this feedback is the duration of the zoom
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ZoomDuration); } set { ZoomDuration = value; } }

		/// <summary>
		/// On Play, triggers a zoom event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData(Channel), Timing.TimescaleMode == TimescaleModes.Unscaled, false, RelativeFieldOfView);
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
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData(Channel), Timing.TimescaleMode == TimescaleModes.Unscaled, stop:true);
		}
	}
}