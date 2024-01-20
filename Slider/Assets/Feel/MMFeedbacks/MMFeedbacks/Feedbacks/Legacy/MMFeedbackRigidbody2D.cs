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
	public class MMFeedbackRigidbody2D : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		public enum Modes { AddForce, AddRelativeForce, AddTorque}

		[Header("Rigidbody")]
		/// the rigidbody to target on play
		[Tooltip("the rigidbody to target on play")]
		public Rigidbody2D TargetRigidbody2D;
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
            
			switch (Mode)
			{
				case Modes.AddForce:
					_force.x = Random.Range(MinForce.x, MaxForce.x);
					_force.y = Random.Range(MinForce.y, MaxForce.y);
					if (!Timing.ConstantIntensity) { _force *= feedbacksIntensity; }
					TargetRigidbody2D.AddForce(_force, AppliedForceMode);
					break;
				case Modes.AddRelativeForce:
					_force.x = Random.Range(MinForce.x, MaxForce.x);
					_force.y = Random.Range(MinForce.y, MaxForce.y);
					if (!Timing.ConstantIntensity) { _force *= feedbacksIntensity; }
					TargetRigidbody2D.AddRelativeForce(_force, AppliedForceMode);
					break;
				case Modes.AddTorque:
					_torque = Random.Range(MinTorque, MaxTorque);
					if (!Timing.ConstantIntensity) { _torque *= feedbacksIntensity; }
					TargetRigidbody2D.AddTorque(_torque, AppliedForceMode);
					break;
			}
		}
	}
}