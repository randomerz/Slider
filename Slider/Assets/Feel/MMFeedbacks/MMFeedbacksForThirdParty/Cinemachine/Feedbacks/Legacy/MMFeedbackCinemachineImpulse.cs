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
	[FeedbackPath("Camera/Cinemachine Impulse")]
	[FeedbackHelp("This feedback lets you trigger a Cinemachine Impulse event. You'll need a Cinemachine Impulse Listener on your camera for this to work.")]
	public class MMFeedbackCinemachineImpulse : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		[Header("Cinemachine Impulse")]
		#if MM_CINEMACHINE
		/// the impulse definition to broadcast
		[Tooltip("the impulse definition to broadcast")]
		[CinemachineImpulseDefinitionProperty]
		public CinemachineImpulseDefinition m_ImpulseDefinition;
		#endif
        
		/// the velocity to apply to the impulse shake
		[Tooltip("the velocity to apply to the impulse shake")]
		public Vector3 Velocity;
		/// whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback
		[Tooltip("whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback")]
		public bool ClearImpulseOnStop = false;

		#if MM_CINEMACHINE
		/// the duration of this feedback is the duration of the impulse
		public override float FeedbackDuration { get { return m_ImpulseDefinition.m_TimeEnvelope.Duration; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}       
			#if MM_CINEMACHINE 
			CinemachineImpulseManager.Instance.IgnoreTimeScale = (Timing.TimescaleMode == TimescaleModes.Unscaled);
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
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
			if (!Active || !FeedbackTypeAuthorized || !ClearImpulseOnStop)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			#if MM_CINEMACHINE
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}
	}
}