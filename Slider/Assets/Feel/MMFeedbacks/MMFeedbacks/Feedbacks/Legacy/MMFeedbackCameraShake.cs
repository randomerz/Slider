using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will send a shake event when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("Define camera shake properties (duration in seconds, amplitude and frequency), and this will broadcast a MMCameraShakeEvent with these same settings. " +
	              "You'll need to add a MMCameraShaker on your camera for this to work (or a MMCinemachineCameraShaker component on your virtual camera if you're using Cinemachine). " +
	              "Note that although this event and system was built for cameras in mind, you could technically use it to shake other objects as well.")]
	[FeedbackPath("Camera/Camera Shake")]
	public class MMFeedbackCameraShake : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		[Header("Camera Shake")]
		/// whether or not this shake should repeat forever, until stopped
		[Tooltip("whether or not this shake should repeat forever, until stopped")]
		public bool RepeatUntilStopped = false;
		/// the channel to broadcast this shake on
		[Tooltip("the channel to broadcast this shake on")]
		public int Channel = 0;
		/// the properties of the shake (duration, intensity, frequenc)
		[Tooltip("the properties of the shake (duration, intensity, frequenc)")]
		public MMCameraShakeProperties CameraShakeProperties = new MMCameraShakeProperties(0.1f, 0.2f, 40f);
        
		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(CameraShakeProperties.Duration); } set { CameraShakeProperties.Duration = value; } }

		/// <summary>
		/// On Play, sends a shake camera event
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
			MMCameraShakeEvent.Trigger(FeedbackDuration, CameraShakeProperties.Amplitude * intensityMultiplier, CameraShakeProperties.Frequency, 
				CameraShakeProperties.AmplitudeX * intensityMultiplier, CameraShakeProperties.AmplitudeY * intensityMultiplier, CameraShakeProperties.AmplitudeZ * intensityMultiplier,
				RepeatUntilStopped, ChannelData(Channel), Timing.TimescaleMode == TimescaleModes.Unscaled);
		}

		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMCameraShakeStopEvent.Trigger(ChannelData(Channel));
		}
	}
}