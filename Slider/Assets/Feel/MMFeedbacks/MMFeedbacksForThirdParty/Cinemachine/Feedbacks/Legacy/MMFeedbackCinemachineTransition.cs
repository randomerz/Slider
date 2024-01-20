using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the priorities of your cameras. 
	/// It requires a bit of setup : adding a MMCinemachinePriorityListener to your different cameras, with unique Channel values on them.
	/// Optionally, you can add a MMCinemachinePriorityBrainListener on your Cinemachine Brain to handle different transition types and durations.
	/// Then all you have to do is pick a channel and a new priority on your feedback, and play it. Magic transition!
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Camera/Cinemachine Transition")]
	[FeedbackHelp("This feedback will let you change the priorities of your cameras. It requires a bit of setup : " +
	              "adding a MMCinemachinePriorityListener to your different cameras, with unique Channel values on them. " +
	              "Optionally, you can add a MMCinemachinePriorityBrainListener on your Cinemachine Brain to handle different transition types and durations. " +
	              "Then all you have to do is pick a channel and a new priority on your feedback, and play it. Magic transition!")]
	public class MMFeedbackCinemachineTransition : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum Modes { Event, Binding }
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif
		/// the duration of this feedback is the duration of the shake
		#if MM_CINEMACHINE
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(BlendDefintion.m_Time); } set { BlendDefintion.m_Time = value; } }
		#endif

		[Header("Cinemachine Transition")] 
		/// the selected mode (either via event, or via direct binding of a specific camera)
		[Tooltip("the selected mode (either via event, or via direct binding of a specific camera)")]
		public Modes Mode = Modes.Event;
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		#if MM_CINEMACHINE
		/// the virtual camera to target
		[Tooltip("the virtual camera to target")]
		[MMFEnumCondition("Mode", (int)Modes.Binding)]
		public CinemachineVirtualCamera TargetVirtualCamera;
		#endif
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetValuesAfterTransition = true;

		[Header("Priority")]
		/// the new priority to apply to all virtual cameras on the specified channel
		[Tooltip("the new priority to apply to all virtual cameras on the specified channel")]
		public int NewPriority = 10;
		/// whether or not to force all virtual cameras on other channels to reset their priority to zero
		[Tooltip("whether or not to force all virtual cameras on other channels to reset their priority to zero")]
		public bool ForceMaxPriority = true;
		/// whether or not to apply a new blend
		[Tooltip("whether or not to apply a new blend")]
		public bool ForceTransition = false;
        
		#if MM_CINEMACHINE
		/// the new blend definition to apply
		[Tooltip("the new blend definition to apply")]
		[MMFCondition("ForceTransition", true)]
		public CinemachineBlendDefinition BlendDefintion;

		protected CinemachineBlendDefinition _tempBlend;
		#endif

		/// <summary>
		/// Triggers a priority change on listening virtual cameras
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			#if MM_CINEMACHINE
			_tempBlend = BlendDefintion;
			_tempBlend.m_Time = FeedbackDuration;
			if (Mode == Modes.Event)
			{
				MMCinemachinePriorityEvent.Trigger(ChannelData(Channel), ForceMaxPriority, NewPriority, ForceTransition, _tempBlend, ResetValuesAfterTransition, Timing.TimescaleMode);    
			}
			else
			{
				MMCinemachinePriorityEvent.Trigger(ChannelData(Channel), ForceMaxPriority, 0, ForceTransition, _tempBlend, ResetValuesAfterTransition, Timing.TimescaleMode); 
				TargetVirtualCamera.Priority = NewPriority;
			}
			#endif
		}
	}
}