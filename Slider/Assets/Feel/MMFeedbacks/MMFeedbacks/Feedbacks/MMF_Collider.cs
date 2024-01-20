using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you enable/disable/toggle a target collider, or change its trigger status
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you enable/disable/toggle a target collider, or change its trigger status")]
	[FeedbackPath("GameObject/Collider")]
	public class MMF_Collider : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetCollider == null); }
		public override string RequiredTargetText { get { return TargetCollider != null ? TargetCollider.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetCollider be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetCollider = FindAutomatedTarget<Collider>();

		/// the possible effects the feedback can have on the target collider's status 
		public enum Modes { Enable, Disable, ToggleActive, Trigger, NonTrigger, ToggleTrigger }

		[MMFInspectorGroup("Collider", true, 12, true)]
		/// the collider to act upon
		[Tooltip("the collider to act upon")]
		public Collider TargetCollider;
		/// the effect the feedback will have on the target collider's status 
		public Modes Mode = Modes.Disable;

		protected bool _initialState;

		/// <summary>
		/// On Play we change the state of our collider if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (TargetCollider != null)
			{
				ApplyChanges(Mode);
			}
		}

		/// <summary>
		/// Changes the state of the collider
		/// </summary>
		/// <param name="state"></param>
		protected virtual void ApplyChanges(Modes mode)
		{
			switch (mode)
			{
				case Modes.Enable:
					_initialState = TargetCollider.enabled;
					TargetCollider.enabled = true;
					break;
				case Modes.Disable:
					_initialState = TargetCollider.enabled;
					TargetCollider.enabled = false;
					break;
				case Modes.ToggleActive:
					_initialState = TargetCollider.enabled;
					TargetCollider.enabled = !TargetCollider.enabled;
					break;
				case Modes.Trigger:
					_initialState = TargetCollider.isTrigger;
					TargetCollider.isTrigger = true;
					break;
				case Modes.NonTrigger:
					_initialState = TargetCollider.isTrigger;
					TargetCollider.isTrigger = false;
					break;
				case Modes.ToggleTrigger:
					_initialState = TargetCollider.isTrigger;
					TargetCollider.isTrigger = !TargetCollider.isTrigger;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
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

			switch (Mode)
			{
				case Modes.Enable:
					TargetCollider.enabled = _initialState;
					break;
				case Modes.Disable:
					TargetCollider.enabled = _initialState;
					break;
				case Modes.ToggleActive:
					TargetCollider.enabled = _initialState;
					break;
				case Modes.Trigger:
					TargetCollider.isTrigger = _initialState;
					break;
				case Modes.NonTrigger:
					TargetCollider.isTrigger = _initialState;
					break;
				case Modes.ToggleTrigger:
					TargetCollider.isTrigger = _initialState;
					break;
			}
		}
	}
}