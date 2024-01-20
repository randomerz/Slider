using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	[AddComponentMenu("")]
	#if MM_CINEMACHINE
	[FeedbackPath("Camera/Cinemachine Impulse")]
	#endif
	[FeedbackHelp("This feedback lets you trigger a Cinemachine Impulse event. You'll need a Cinemachine Impulse Listener on your camera for this to work.")]
	public class MMF_CinemachineImpulse : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif
		public override bool HasRandomness => true;

		#if MM_CINEMACHINE
		[MMFInspectorGroup("Cinemachine Impulse", true, 28)]
		/// the impulse definition to broadcast
		[Tooltip("the impulse definition to broadcast")]
		public CinemachineImpulseDefinition m_ImpulseDefinition;
		/// the velocity to apply to the impulse shake
		[Tooltip("the velocity to apply to the impulse shake")]
		public Vector3 Velocity;
		/// whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback
		[Tooltip("whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback")]
		public bool ClearImpulseOnStop = false;

		/// the duration of this feedback is the duration of the impulse
		public override float FeedbackDuration { get { return m_ImpulseDefinition != null ? m_ImpulseDefinition.m_TimeEnvelope.Duration : 0f; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_CINEMACHINE
			CinemachineImpulseManager.Instance.IgnoreTimeScale = !InScaledTimescaleMode;
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			m_ImpulseDefinition.CreateEvent(position, Velocity * intensityMultiplier);
			#endif
		}

		/// <summary>
		/// Stops the animation if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			#if MM_CINEMACHINE
			if (!Active || !FeedbackTypeAuthorized || !ClearImpulseOnStop)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}
	}
}