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
	[FeedbackPath("GameObject/Rigidbody")]
	public class MMF_Rigidbody : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRigidbody == null); }
		public override string RequiredTargetText { get { return TargetRigidbody != null ? TargetRigidbody.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRigidbody be set to be able to work properly. You can set one below."; } }
		#endif
		public enum Modes { AddForce, AddRelativeForce, AddTorque, AddRelativeTorque }
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRigidbody = FindAutomatedTarget<Rigidbody>();

		[MMFInspectorGroup("Rigidbody", true, 61, true)]
		/// the rigidbody to target on play
		[Tooltip("the rigidbody to target on play")]
		public Rigidbody TargetRigidbody;
		/// a list of extra rigidbodies to target on play
		[Tooltip("a list of extra rigidbodies to target on play")]
		public List<Rigidbody> ExtraTargetRigidbodies;
		/// the selected mode for this feedback
		[Tooltip("the selected mode for this feedback")]
		public Modes Mode = Modes.AddForce;
		/// the min force or torque to apply
		[Tooltip("the min force or torque to apply")]
		public Vector3 MinForce;
		/// the max force or torque to apply
		[Tooltip("the max force or torque to apply")]
		public Vector3 MaxForce;
		/// the force mode to apply
		[Tooltip("the force mode to apply")]
		public ForceMode AppliedForceMode = ForceMode.Impulse;

		protected Vector3 _force;

		/// <summary>
		/// On Custom Play, we apply our force or torque to the target rigidbody
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetRigidbody == null))
			{
				return;
			}

			_force.x = Random.Range(MinForce.x, MaxForce.x);
			_force.y = Random.Range(MinForce.y, MaxForce.y);
			_force.z = Random.Range(MinForce.z, MaxForce.z);

			if (!Timing.ConstantIntensity)
			{
				_force *= feedbacksIntensity;
			}
			
			ApplyForce(TargetRigidbody);
			foreach (Rigidbody rb in ExtraTargetRigidbodies)
			{
				ApplyForce(rb);
			}
		}

		/// <summary>
		/// Applies the computed force to the target rigidbody
		/// </summary>
		/// <param name="rb"></param>
		protected virtual void ApplyForce(Rigidbody rb)
		{
			switch (Mode)
			{
				case Modes.AddForce:
					rb.AddForce(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeForce:
					rb.AddRelativeForce(_force, AppliedForceMode);
					break;
				case Modes.AddTorque:
					rb.AddTorque(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeTorque:
					rb.AddRelativeTorque(_force, AppliedForceMode);
					break;
			}
		}
	}
}