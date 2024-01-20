using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// this feedback will let you animate the position of 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will animate the target object's position over time, for the specified duration, from the chosen initial position to the chosen destination. These can either be relative Vector3 offsets from the Feedback's position, or Transforms. If you specify transforms, the Vector3 values will be ignored.")]
	[FeedbackPath("Transform/Position")]
	public class MMF_Position : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor => MMFeedbacksInspectorColors.TransformColor; 
		public override bool EvaluateRequiresSetup() => (AnimatePositionTarget == null);
		public override string RequiredTargetText => AnimatePositionTarget != null ? AnimatePositionTarget.name : "";  
		public override string RequiresSetupText => "This feedback requires that a AnimatePositionTarget and a Destination be set to be able to work properly. You can set one below."; 
		public override bool HasCustomInspectors => true; 
		#endif
		public override bool HasRandomness => true;
		public override bool CanForceInitialValue => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => AnimatePositionTarget = FindAutomatedTargetGameObject();
		
		public enum Spaces { World, Local, RectTransform, Self }
		public enum Modes { AtoB, AlongCurve, ToDestination }
		public enum TimeScales { Scaled, Unscaled }

		[MMFInspectorGroup("Position Target", true, 61, true)]
		/// the object this feedback will animate the position for
		[Tooltip("the object this feedback will animate the position for")]
		public GameObject AnimatePositionTarget;

		[MMFInspectorGroup("Transition", true, 63)]
		/// the mode this animation should follow (either going from A to B, or moving along a curve)
		[Tooltip("the mode this animation should follow (either going from A to B, or moving along a curve)")]
		public Modes Mode = Modes.AtoB;
		/// the space in which to move the position in
		[Tooltip("the space in which to move the position in")]
		public Spaces Space = Spaces.World;
		
		/// whether or not to randomize remap values between their base and alt values on play, useful to add some variety every time you play this feedback
		[Tooltip("whether or not to randomize remap values between their base and alt values on play, useful to add some variety every time you play this feedback")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool RandomizeRemap = false;
		/// the duration of the animation on play
		[Tooltip("the duration of the animation on play")]
		public float AnimatePositionDuration = 0.2f;
		
		/// the MMTween curve definition to use instead of the animation curve to define the acceleration of the movement
		[Tooltip("the MMTween curve definition to use instead of the animation curve to define the acceleration of the movement")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public MMTweenType AnimatePositionTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		
		/// the value to remap the curve's 0 value to
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[Tooltip("the value to remap the curve's 0 value to")]
		public float RemapCurveZero = 0f;
		/// in randomize remap mode, the value to remap the curve's 0 value to (randomized between this and RemapCurveZero)
		[MMFCondition("RandomizeRemap", true)]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[Tooltip("in randomize remap mode, the value to remap the curve's 0 value to (randomized between this and RemapCurveZero")]
		public float RemapCurveZeroAlt = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[FormerlySerializedAs("CurveMultiplier")]
		public float RemapCurveOne = 1f;
		/// in randomize remap mode, the value to remap the curve's 1 value to (randomized between this and RemapCurveOne)
		[Tooltip("in randomize remap mode, the value to remap the curve's 1 value to (randomized between this and RemapCurveOne)")]
		[MMFCondition("RandomizeRemap", true)]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public float RemapCurveOneAlt = 1f;
		/// if this is true, the x position will be animated
		[Tooltip("if this is true, the x position will be animated")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateX;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFCondition("AnimateX", true)]
		public MMTweenType AnimatePositionTweenX = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f)));
		/// if this is true, the y position will be animated
		[Tooltip("if this is true, the y position will be animated")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateY;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFCondition("AnimateY", true)]
		public MMTweenType AnimatePositionTweenY = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f)));
		/// if this is true, the z position will be animated
		[Tooltip("if this is true, the z position will be animated")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateZ;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFCondition("AnimateZ", true)]
		public MMTweenType AnimatePositionTweenZ = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f)));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		[MMFInspectorGroup("Positions", true, 64)]
		/// if this is true, movement will be relative to the object's initial position. So moving its y position along a curve going from 0 to 1 will move it up one unit. If this is false, in that same example, it'll be moved from 0 to 1 in absolute coordinates.
		[Tooltip("if this is true, movement will be relative to the object's initial position. So moving its y position along a curve going from 0 to 1 will move it up one unit. If this is false, in that same example, it'll be moved from 0 to 1 in absolute coordinates.")]
		public bool RelativePosition = true;
		/// if this is true, initial and destination positions will be recomputed on every play
		[Tooltip("if this is true, initial and destination positions will be recomputed on every play")]
		public bool DeterminePositionsOnPlay = false;
		/// the initial position
		[Tooltip("the initial position")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.AlongCurve)]
		public Vector3 InitialPosition = Vector3.zero;
		/// the destination position
		[Tooltip("the destination position")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public Vector3 DestinationPosition = Vector3.one;
		/// the initial transform - if set, takes precedence over the Vector3 above
		[Tooltip("the initial transform - if set, takes precedence over the Vector3 above")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.AlongCurve)]
		public Transform InitialPositionTransform;
		/// the destination transform - if set, takes precedence over the Vector3 above
		[Tooltip("the destination transform - if set, takes precedence over the Vector3 above")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public Transform DestinationPositionTransform;
		/// the duration of this feedback is the duration of its animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimatePositionDuration); } set { AnimatePositionDuration = value; } }
		
		/// [DEPRECATED] the acceleration of the movement
		[HideInInspector] public AnimationCurve AnimatePositionCurveX = null;
		/// [DEPRECATED] the acceleration of the movement
		[HideInInspector] public AnimationCurve AnimatePositionCurveY = null;
		/// [DEPRECATED] the acceleration of the movement
		[HideInInspector] public AnimationCurve AnimatePositionCurveZ = null;
		/// [DEPRECATED] the acceleration of the movement - this is not used anymore, replaced by the AnimatePositionTween
		[HideInInspector] public AnimationCurve AnimatePositionCurve = null;

		protected Vector3 _newPosition;
		protected Vector3 _currentPosition;
		protected RectTransform _rectTransform;
		protected Vector3 _initialPosition;
		protected Vector3 _destinationPosition;
		protected Coroutine _coroutine;
		protected Vector3 _workInitialPosition;
		protected Vector3 _workDestinationPosition;
		protected float _remapCurveZero;
		protected float _remapCurveOne;

		/// <summary>
		/// On init, we set our initial and destination positions (transform will take precedence over vector3s)
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active)
			{
				if (AnimatePositionTarget == null)
				{
					Debug.LogWarning("The animate position target for " + this + " is null, you have to define it in the inspector");
					return;
				}

				if (Space == Spaces.RectTransform)
				{
					_rectTransform = AnimatePositionTarget.GetComponent<RectTransform>();
				}

				if (!DeterminePositionsOnPlay)
				{
					DeterminePositions();    
				}
			}
		}

		protected virtual void DeterminePositions()
		{
			if (DeterminePositionsOnPlay && RelativePosition && (InitialPosition != Vector3.zero))
			{
				return;
			}
            
			if (InitialPositionTransform != null)
			{
				_workInitialPosition = GetPosition(InitialPositionTransform);
			}
			else
			{
				_workInitialPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + InitialPosition : InitialPosition;
				if (Space == Spaces.Self && !RelativePosition)
				{
					_workInitialPosition = AnimatePositionTarget.transform.position + InitialPosition;
				}
			}
			if (DestinationPositionTransform != null)
			{
				_workDestinationPosition = GetPosition(DestinationPositionTransform);
			}
			else
			{
				_workDestinationPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + DestinationPosition : DestinationPosition;
			}
		}

		/// <summary>
		/// On Play, we move our object from A to B
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimatePositionTarget == null))
			{
				return;
			}
            
			if (Active || Owner.AutoPlayOnEnable)
			{
				if (DeterminePositionsOnPlay && NormalPlayDirection)
				{
					DeterminePositions();
				}
                
				switch (Mode)
				{
					case Modes.ToDestination:
						_initialPosition = GetPosition(AnimatePositionTarget.transform);
						_destinationPosition = _workDestinationPosition;
						if (DestinationPositionTransform != null)
						{
							_destinationPosition = GetPosition(DestinationPositionTransform);
						}
						_coroutine = Owner.StartCoroutine(MoveFromTo(AnimatePositionTarget, _initialPosition, _destinationPosition, FeedbackDuration, AnimatePositionTween));
						break;
					case Modes.AtoB:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = Owner.StartCoroutine(MoveFromTo(AnimatePositionTarget, _workInitialPosition, _workDestinationPosition, FeedbackDuration, AnimatePositionTween));
						break;
					case Modes.AlongCurve:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

						_remapCurveZero = RandomizeRemap ? Random.Range(RemapCurveZero, RemapCurveZeroAlt) : RemapCurveZero;
						_remapCurveOne = RandomizeRemap ? Random.Range(RemapCurveOne, RemapCurveOneAlt) : RemapCurveOne;
						
						_coroutine = Owner.StartCoroutine(MoveAlongCurve(AnimatePositionTarget, _workInitialPosition, FeedbackDuration, intensityMultiplier));
						break;
				}                    
			}
		}

		/// <summary>
		/// Moves the object along a curve
		/// </summary>
		/// <param name="movingObject"></param>
		/// <param name="pointA"></param>
		/// <param name="pointB"></param>
		/// <param name="duration"></param>
		/// <param name="curve"></param>
		/// <returns></returns>
		protected virtual IEnumerator MoveAlongCurve(GameObject movingObject, Vector3 initialPosition, float duration, float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);

				ComputeNewCurvePosition(movingObject, initialPosition, percent, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			ComputeNewCurvePosition(movingObject, initialPosition, FinalNormalizedTime, intensityMultiplier);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// Evaluates the position curves and computes the new position
		/// </summary>
		/// <param name="movingObject"></param>
		/// <param name="initialPosition"></param>
		/// <param name="percent"></param>
		protected virtual void ComputeNewCurvePosition(GameObject movingObject, Vector3 initialPosition, float percent, float intensityMultiplier)
		{
			float newValueX = MMTween.Tween(percent, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier, AnimatePositionTweenX);
			float newValueY = MMTween.Tween(percent, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier, AnimatePositionTweenY);
			float newValueZ = MMTween.Tween(percent, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier, AnimatePositionTweenZ);

			_newPosition = initialPosition;
			_currentPosition = GetPosition(movingObject.transform);

			if (RelativePosition)
			{
				_newPosition.x = AnimateX ? initialPosition.x + newValueX : _currentPosition.x;
				_newPosition.y = AnimateY ? initialPosition.y + newValueY : _currentPosition.y;
				_newPosition.z = AnimateZ ? initialPosition.z + newValueZ : _currentPosition.z;
			}
			else
			{
				_newPosition.x = AnimateX ? newValueX : _currentPosition.x;
				_newPosition.y = AnimateY ? newValueY : _currentPosition.y;
				_newPosition.z = AnimateZ ? newValueZ : _currentPosition.z;
			}

			if (Space == Spaces.Self)
			{
				_newPosition.x = AnimateX ? newValueX : 0f;
				_newPosition.y = AnimateY ? newValueY : 0f;
				_newPosition.z = AnimateZ ? newValueZ : 0f;
			}
			
			SetPosition(movingObject.transform, _newPosition);
		}

		/// <summary>
		/// Moves an object from point A to point B in a given time
		/// </summary>
		/// <param name="movingObject">Moving object.</param>
		/// <param name="pointA">Point a.</param>
		/// <param name="pointB">Point b.</param>
		/// <param name="duration">Time.</param>
		protected virtual IEnumerator MoveFromTo(GameObject movingObject, Vector3 pointA, Vector3 pointB, float duration, MMTweenType tweenType)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float curveValue = MMTween.Tween(journey, 0f, duration, 0f, 1f, tweenType);
				
				_newPosition = Vector3.LerpUnclamped(pointA, pointB, curveValue);
				SetPosition(movingObject.transform, _newPosition);
				
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			// set final position
			if (NormalPlayDirection)
			{
				SetPosition(movingObject.transform, pointB);    
			}
			else
			{
				SetPosition(movingObject.transform, pointA);
			}
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// Gets the world, local or anchored position
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Transform target)
		{
			switch (Space)
			{
				case Spaces.World:
					return target.position;
				case Spaces.Local:
					return target.localPosition;
				case Spaces.RectTransform:
					return target.gameObject.GetComponent<RectTransform>().anchoredPosition;
				case Spaces.Self:
					return target.position;
			}
			return Vector3.zero;
		}

		/// <summary>
		/// Sets the position, localposition or anchoredposition of the target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="newPosition"></param>
		protected virtual void SetPosition(Transform target, Vector3 newPosition)
		{
			switch (Space)
			{
				case Spaces.World:
					target.position = newPosition;
					break;
				case Spaces.Local:
					target.localPosition = newPosition;
					break;
				case Spaces.RectTransform:
					_rectTransform.anchoredPosition = newPosition;
					break;
				case Spaces.Self:
					target.position = _workInitialPosition;
					if ((Mode == Modes.AtoB) || (Mode == Modes.ToDestination))
					{
						newPosition -= _workInitialPosition;
					}
					target.Translate(newPosition, target);
					break;
			}
		}

		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
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
			
			SetPosition(AnimatePositionTarget.transform, _workInitialPosition);
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
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurve, AnimatePositionTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurveX, AnimatePositionTweenX, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurveY, AnimatePositionTweenY, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurveZ, AnimatePositionTweenZ, Owner);
		}
	}
}