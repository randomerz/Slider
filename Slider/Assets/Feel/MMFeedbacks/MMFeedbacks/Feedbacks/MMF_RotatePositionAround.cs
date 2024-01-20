using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will animate the target's position (not its rotation), on an arc around the specified rotation center, for the specified duration (in seconds).
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will animate the target's position (not its rotation), on an arc around the specified rotation center, for the specified duration (in seconds).")]
	[FeedbackPath("Transform/Rotate Position Around")]
	public class MMF_RotatePositionAround : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the timescale modes this feedback can operate on
		public enum TimeScales { Scaled, Unscaled }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateRotationTarget == null); }
		public override string RequiredTargetText { get { return ((AnimateRotationTarget != null) || (AnimateRotationCenter != null)) ? AnimateRotationTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a AnimatePositionTarget and a AnimateRotationCenter be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => AnimateRotationTarget = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Animation Targets", true, 61, true)]
		/// the object whose rotation you want to animate
		[Tooltip("the object whose rotation you want to animate")]
		public Transform AnimateRotationTarget;
		/// the object around which to rotate AnimateRotationTarget
		[Tooltip("the object around which to rotate AnimateRotationTarget")]
		public Transform AnimateRotationCenter;
		
		[MMFInspectorGroup("Transition", true, 63)]
		/// the duration of the transition
		[Tooltip("the duration of the transition")]
		public float AnimateRotationDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("the value to remap the curve's 0 value to")]
		public float RemapCurveZero = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		public float RemapCurveOne = 180f;
		/// if this is true, should animate movement on the X axis
		[Tooltip("if this is true, should animate movement on the X axis")]
		public bool AnimateX = false;
		/// how the x part of the movement should animate over time, in degrees
		[Tooltip("how the x part of the movement should animate over time, in degrees")]
		[MMCondition("AnimateX", true)]
		public AnimationCurve AnimateRotationX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, should animate movement on the Y axis
		[Tooltip("if this is true, should animate movement on the Y axis")]
		public bool AnimateY = true;
		/// how the y part of the rotation should animate over time, in degrees
		[Tooltip("how the y part of the rotation should animate over time, in degrees")]
		[MMCondition("AnimateY", true)]
		public AnimationCurve AnimateRotationY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, should animate movement on the Z axis
		[Tooltip("if this is true, should animate movement on the Z axis")]
		public bool AnimateZ = false;
		/// how the z part of the rotation should animate over time, in degrees
		[Tooltip("how the z part of the rotation should animate over time, in degrees")]
		[MMCondition("AnimateZ", true)]
		public AnimationCurve AnimateRotationZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination rotations will be recomputed on every play
		[Tooltip("if this is true, initial and destination rotations will be recomputed on every play")]
		public bool DetermineRotationOnPlay = false;
        
		/// the duration of this feedback is the duration of the rotation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateRotationDuration); } set { AnimateRotationDuration = value; } }
		public override bool HasRandomness => true;

		protected Vector3 _initialPosition;
		protected Vector3 _rotationAngles;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial rotation
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			
			if (Active && (AnimateRotationTarget != null))
			{
				GetInitialPosition();
			}
		}

		/// <summary>
		/// Stores initial rotation for future use
		/// </summary>
		protected virtual void GetInitialPosition()
		{
			_initialPosition = AnimateRotationTarget.transform.position;
		}

		/// <summary>
		/// On play, we trigger our rotation animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateRotationTarget == null))
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (Active || Owner.AutoPlayOnEnable)
			{
				if (!AllowAdditivePlays && (_coroutine != null))
				{
					return;
				}
				if (DetermineRotationOnPlay && NormalPlayDirection) { GetInitialPosition(); }
				ClearCoroutine();
				_coroutine = Owner.StartCoroutine(AnimateRotation(AnimateRotationTarget, Vector3.zero, FeedbackDuration, AnimateRotationX, AnimateRotationY, AnimateRotationZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
			}
		}

		protected virtual void ClearCoroutine()
		{
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

		/// <summary>
		/// A coroutine used to compute the rotation over time
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="vector"></param>
		/// <param name="duration"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateRotation(Transform targetTransform,
			Vector3 vector,
			float duration,
			AnimationCurve curveX,
			AnimationCurve curveY,
			AnimationCurve curveZ,
			float remapZero,
			float remapOne)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if ((curveX == null) || (curveY == null) || (curveZ == null))
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;

			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
                
				ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, percent);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
            
			yield break;
		}
		
		/// <summary>
		/// Computes and applies the rotation to the object
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="multiplier"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="percent"></param> 
		protected virtual void ApplyRotation(Transform targetTransform, float remapZero, float remapOne, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float percent)
		{
			targetTransform.position = _initialPosition;

			_rotationAngles.x = 0f;
			_rotationAngles.y = 0f;
			_rotationAngles.z= 0f;
			
			if (AnimateX)
			{
				_rotationAngles.x = curveX.Evaluate(percent);
				_rotationAngles.x = MMFeedbacksHelpers.Remap(_rotationAngles.x, 0f, 1f, remapZero, remapOne);
			}
			if (AnimateY)
			{
				_rotationAngles.y = curveY.Evaluate(percent);
				_rotationAngles.y = MMFeedbacksHelpers.Remap(_rotationAngles.y, 0f, 1f, remapZero, remapOne);
			}
			if (AnimateZ)
			{
				_rotationAngles.z = curveZ.Evaluate(percent);
				_rotationAngles.z = MMFeedbacksHelpers.Remap(_rotationAngles.z, 0f, 1f, remapZero, remapOne);
			}

			targetTransform.position = MMMaths.RotatePointAroundPivot(targetTransform.position, AnimateRotationCenter.position, _rotationAngles);
		}
        
		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (_coroutine != null))
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
				IsPlaying = false;
			}
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			AnimateRotationTarget.transform.position = _initialPosition;
		}

		/// <summary>
		/// On disable we reset our coroutine
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}
	}
}