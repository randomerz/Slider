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
	public class MMFeedbackCollider : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		/// the possible effects the feedback can have on the target collider's status 
		public enum Modes { Enable, Disable, ToggleActive, Trigger, NonTrigger, ToggleTrigger }

		[Header("Collider")]
		/// the collider to act upon
		[Tooltip("the collider to act upon")]
		public Collider TargetCollider;
		/// the effect the feedback will have on the target collider's status 
		public Modes Mode = Modes.Disable;

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
					TargetCollider.enabled = true;
					break;
				case Modes.Disable:
					TargetCollider.enabled = false;
					break;
				case Modes.ToggleActive:
					TargetCollider.enabled = !TargetCollider.enabled;
					break;
				case Modes.Trigger:
					TargetCollider.isTrigger = true;
					break;
				case Modes.NonTrigger:
					TargetCollider.isTrigger = false;
					break;
				case Modes.ToggleTrigger:
					TargetCollider.isTrigger = !TargetCollider.isTrigger;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}
	}
}