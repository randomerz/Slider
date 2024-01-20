using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// this feedback will let you apply forces and torques (relative or not) to a Rigidbody
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you apply forces and torques (relative or not) to a Rigidbody.")]
	[FeedbackPath("GameObject/Rigidbody2D")]
	public class MMF_Rigidbody2D : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRigidbody2D == null); }
		public override string RequiredTargetText { get { return TargetRigidbody2D != null ? TargetRigidbody2D.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRigidbody2D be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRigidbody2D = FindAutomatedTarget<Rigidbody2D>();

		public enum Modes { AddForce, AddRelativeForce, AddTorque}

		[MMFInspectorGroup("Rigidbody2D", true, 32, true)]
		/// the rigidbody to target on play
		[Tooltip("the rigidbody to target on play")]
		public Rigidbody2D TargetRigidbody2D;
		/// an extra list of rigidbodies to target on play
		[Tooltip("an extra list of rigidbodies to target on play")]
		public List<Rigidbody2D> ExtraTargetRigidbodies2D;
		/// the selected mode for this feedback
		[Tooltip("the selected mode for this feedback")]
		public Modes Mode = Modes.AddForce;
		/// the min force or torque to apply
		[Tooltip("the min force or torque to apply")]
		[MMFEnumCondition("Mode", (int)Modes.AddForce, (int)Modes.AddRelativeForce)]
		public Vector2 MinForce;
		/// the max force or torque to apply
		[Tooltip("the max force or torque to apply")]
		[MMFEnumCondition("Mode", (int)Modes.AddForce, (int)Modes.AddRelativeForce)]
		public Vector2 MaxForce;
		/// the min torque to apply to this rigidbody on play
		[Tooltip("the min torque to apply to this rigidbody on play")]
		[MMFEnumCondition("Mode", (int)Modes.AddTorque)]
		public float MinTorque;
		/// the max torque to apply to this rigidbody on play
		[Tooltip("the max torque to apply to this rigidbody on play")]
		[MMFEnumCondition("Mode", (int)Modes.AddTorque)]
		public float MaxTorque;
		/// the force mode to apply
		[Tooltip("the force mode to apply")]
		public ForceMode2D AppliedForceMode = ForceMode2D.Impulse;

		protected Vector2 _force;
		protected float _torque;

		/// <summary>
		/// On Custom Play, we apply our force or torque to the target rigidbody
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetRigidbody2D == null))
			{
				return;
			}
			
			ApplyForce(TargetRigidbody2D, feedbacksIntensity);
			foreach (Rigidbody2D rb in ExtraTargetRigidbodies2D)
			{
				ApplyForce(rb, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Applies the computed force to the target rigidbody
		/// </summary>
		/// <param name="rb"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void ApplyForce(Rigidbody2D rb, float feedbacksIntensity)
		{
			switch (Mode)
			{
				case Modes.AddForce:
					_force.x = Random.Range(MinForce.x, MaxForce.x);
					_force.y = Random.Range(MinForce.y, MaxForce.y);
					if (!Timing.ConstantIntensity) { _force *= feedbacksIntensity; }
					rb.AddForce(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeForce:
					_force.x = Random.Range(MinForce.x, MaxForce.x);
					_force.y = Random.Range(MinForce.y, MaxForce.y);
					if (!Timing.ConstantIntensity) { _force *= feedbacksIntensity; }
					rb.AddRelativeForce(_force, AppliedForceMode);
					break;
				case Modes.AddTorque:
					_torque = Random.Range(MinTorque, MaxTorque);
					if (!Timing.ConstantIntensity) { _torque *= feedbacksIntensity; }
					rb.AddTorque(_torque, AppliedForceMode);
					break;
			}
		}
	}
}