using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback animates the rotation of the specified object when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will animate the target's rotation on the 3 specified animation curves (one per axis), for the specified duration (in seconds).")]
	[FeedbackPath("Transform/Rotation")]
	public class MMF_Rotation : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes for this feedback (Absolute : always follow the curve from start to finish, Additive : add to the values found when this feedback gets played)
		public enum Modes { Absolute, Additive, ToDestination }
		/// the timescale modes this feedback can operate on
		public enum TimeScales { Scaled, Unscaled }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateRotationTarget == null); }
		public override string RequiredTargetText { get { return AnimateRotationTarget != null ? AnimateRotationTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a AnimatePositionTarget and a Destination be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimateRotationTarget = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Rotation Target", true, 61, true)]
		/// the object whose rotation you want to animate
		[Tooltip("the object whose rotation you want to animate")]
		public Transform AnimateRotationTarget;

		[MMFInspectorGroup("Transition", true, 63)]
		/// whether this feedback should animate in absolute values or additive
		[Tooltip("whether this feedback should animate in absolute values or additive")]
		public Modes Mode = Modes.Absolute;
		/// whether this feedback should play on local or world rotation
		[Tooltip("whether this feedback should play on local or world rotation")]
		public Space RotationSpace = Space.World;
		/// the duration of the transition
		[Tooltip("the duration of the transition")]
		public float AnimateRotationDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("the value to remap the curve's 0 value to")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public float RemapCurveZero = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public float RemapCurveOne = 360f;
		/// if this is true, should animate the X rotation
		[Tooltip("if this is true, should animate the X rotation")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateX = true;
		
		
		/// how the x part of the rotation should animate over time, in degrees
		[Tooltip("how the x part of the rotation should animate over time, in degrees")]
		[MMFCondition("AnimateX")]
		public MMTweenType AnimateRotationTweenX = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// if this is true, should animate the Y rotation
		[Tooltip("if this is true, should animate the Y rotation")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateY = true;
		/// how the y part of the rotation should animate over time, in degrees
		[Tooltip("how the y part of the rotation should animate over time, in degrees")]
		[MMFCondition("AnimateY")]
		public MMTweenType AnimateRotationTweenY = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// if this is true, should animate the Z rotation
		[Tooltip("if this is true, should animate the Z rotation")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateZ = true;
		/// how the z part of the rotation should animate over time, in degrees
		[Tooltip("how the z part of the rotation should animate over time, in degrees")]
		[MMFCondition("AnimateZ")]
		public MMTweenType AnimateRotationTweenZ = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		
		
		
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination rotations will be recomputed on every play
		[Tooltip("if this is true, initial and destination rotations will be recomputed on every play")]
		public bool DetermineRotationOnPlay = false;
        
		[Header("To Destination")]
		/// the space in which the ToDestination mode should operate 
		[Tooltip("the space in which the ToDestination mode should operate")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Space ToDestinationSpace = Space.World;
		/// the angles to match when in ToDestination mode
		[Tooltip("the angles to match when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector3 DestinationAngles = new Vector3(0f, 180f, 0f);
		/// how the x part of the rotation should animate over time, in degrees
		[Tooltip("how the x part of the rotation should animate over time, in degrees")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public MMTweenType ToDestinationTween = new MMTweenType(MMTween.MMTweenCurve.EaseInQuintic);
		
		/// the duration of this feedback is the duration of the rotation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateRotationDuration); } set { AnimateRotationDuration = value; } }
		public override bool HasRandomness => true;
		
		/// [DEPRECATED] how the x part of the rotation should animate over time, in degrees
		[HideInInspector] public AnimationCurve AnimateRotationX = null;
		/// [DEPRECATED] how the y part of the rotation should animate over time, in degrees
		[HideInInspector] public AnimationCurve AnimateRotationY = null;
		/// [DEPRECATED] how the z part of the rotation should animate over time, in degrees
		[HideInInspector] public AnimationCurve AnimateRotationZ = null;
		/// [DEPRECATED] the animation curve to use when animating to destination (individual x,y,z curves above won't be used)
		[HideInInspector] public AnimationCurve ToDestinationCurve = null;

		protected Quaternion _initialRotation;
		protected Vector3 _initialToDestinationAngles;
		protected Quaternion _destinationRotation;
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
				GetInitialRotation();
			}
		}

		/// <summary>
		/// Stores initial rotation for future use
		/// </summary>
		protected virtual void GetInitialRotation()
		{
			_initialRotation = (RotationSpace == Space.World) ? AnimateRotationTarget.rotation : AnimateRotationTarget.localRotation;
			_initialToDestinationAngles = _initialRotation.eulerAngles;
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
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (DetermineRotationOnPlay && NormalPlayDirection) { GetInitialRotation(); }
					ClearCoroutine();
					_coroutine = Owner.StartCoroutine(AnimateRotation(AnimateRotationTarget, Vector3.zero, FeedbackDuration, AnimateRotationTweenX, AnimateRotationTweenY, AnimateRotationTweenZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				else if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (DetermineRotationOnPlay && NormalPlayDirection) { GetInitialRotation(); }
					ClearCoroutine();
					_coroutine = Owner.StartCoroutine(RotateToDestination());
				}
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
		/// A coroutine used to rotate the target to its destination rotation
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RotateToDestination()
		{
			if (AnimateRotationTarget == null)
			{
				yield break;
			}

			if ((AnimateRotationTweenX == null) || (AnimateRotationTweenY == null) || (AnimateRotationTweenZ == null))
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			Vector3 destinationAngles = NormalPlayDirection ? DestinationAngles : _initialToDestinationAngles;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialRotation = AnimateRotationTarget.transform.rotation;
			if (ToDestinationSpace == Space.Self)
			{
				AnimateRotationTarget.transform.localRotation = Quaternion.Euler(destinationAngles);
			}
			else
			{
				AnimateRotationTarget.transform.rotation = Quaternion.Euler(destinationAngles);
			}
            
			_destinationRotation = AnimateRotationTarget.transform.rotation;
			AnimateRotationTarget.transform.rotation = _initialRotation;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);
				percent = ToDestinationTween.Evaluate(percent);

				Quaternion newRotation = Quaternion.LerpUnclamped(_initialRotation, _destinationRotation, percent);
				AnimateRotationTarget.transform.rotation = newRotation;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			if (ToDestinationSpace == Space.Self)
			{
				AnimateRotationTarget.transform.localRotation = Quaternion.Euler(destinationAngles);
			}
			else
			{
				AnimateRotationTarget.transform.rotation = Quaternion.Euler(destinationAngles);
			}
			IsPlaying = false;
			_coroutine = null;
			yield break;
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
			MMTweenType curveX,
			MMTweenType curveY,
			MMTweenType curveZ,
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

			if (Mode == Modes.Additive)
			{
				_initialRotation = (RotationSpace == Space.World) ? targetTransform.rotation : targetTransform.localRotation;
			}

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
		protected virtual void ApplyRotation(Transform targetTransform, float remapZero, float remapOne, MMTweenType curveX, MMTweenType curveY, MMTweenType curveZ, float percent)
		{
			if (RotationSpace == Space.World)
			{
				targetTransform.transform.rotation = _initialRotation;    
			}
			else
			{
				targetTransform.transform.localRotation = _initialRotation;
			}

			if (AnimateX)
			{
				float x = MMTween.Tween(percent, 0f, 1f, remapZero, remapOne, curveX);
				targetTransform.Rotate(Vector3.right, x, RotationSpace);
			}
			if (AnimateY)
			{
				float y = MMTween.Tween(percent, 0f, 1f, remapZero, remapOne, curveY);
				targetTransform.Rotate(Vector3.up, y, RotationSpace);
			}
			if (AnimateZ)
			{
				float z = MMTween.Tween(percent, 0f, 1f, remapZero, remapOne, curveZ);
				targetTransform.Rotate(Vector3.forward, z, RotationSpace);
			}
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
		/// On disable we reset our coroutine
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}
		
		/// <summary>
		/// On Validate, we migrate our deprecated animation curves to our tween types if needed
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationX, AnimateRotationTweenX, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationY, AnimateRotationTweenY, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationZ, AnimateRotationTweenZ, Owner);
			MMFeedbacksHelpers.MigrateCurve(ToDestinationCurve, ToDestinationTween, Owner);
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

			if (RotationSpace == Space.World)
			{
				AnimateRotationTarget.rotation = _initialRotation;
			}
			else
			{
				AnimateRotationTarget.localRotation= _initialRotation;	
			}
		}
	}
}