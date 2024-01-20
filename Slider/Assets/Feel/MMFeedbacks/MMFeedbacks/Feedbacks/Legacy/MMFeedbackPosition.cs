using System.Collections;
using System.Collections.Generic;
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
	public class MMFeedbackPosition : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		#endif
		public enum Spaces { World, Local, RectTransform }
		public enum Modes { AtoB, AlongCurve, ToDestination }
		public enum TimeScales { Scaled, Unscaled }

		[Header("Position Target")]
		/// the object this feedback will animate the position for
		[Tooltip("the object this feedback will animate the position for")]
		public GameObject AnimatePositionTarget;

		[Header("Animation")]
		/// the mode this animation should follow (either going from A to B, or moving along a curve)
		[Tooltip("the mode this animation should follow (either going from A to B, or moving along a curve)")]
		public Modes Mode = Modes.AtoB;
		/// whether this feedback should play in scaled or unscaled time
		[Tooltip("whether this feedback should play in scaled or unscaled time")]
		public TimeScales TimeScale = TimeScales.Scaled;
		/// the space in which to move the position in
		[Tooltip("the space in which to move the position in")]
		public Spaces Space = Spaces.World;
		/// the duration of the animation on play
		[Tooltip("the duration of the animation on play")]
		public float AnimatePositionDuration = 0.2f;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		public AnimationCurve AnimatePositionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		/// the value to remap the curve's 0 value to
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[Tooltip("the value to remap the curve's 0 value to")]
		public float RemapCurveZero = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[FormerlySerializedAs("CurveMultiplier")]
		public float RemapCurveOne = 1f;
		/// if this is true, the x position will be animated
		[Tooltip("if this is true, the x position will be animated")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateX;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFCondition("AnimateX", true)]
		public AnimationCurve AnimatePositionCurveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// if this is true, the y position will be animated
		[Tooltip("if this is true, the y position will be animated")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateY;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFCondition("AnimateY", true)]
		public AnimationCurve AnimatePositionCurveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// if this is true, the z position will be animated
		[Tooltip("if this is true, the z position will be animated")]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		public bool AnimateZ;
		/// the acceleration of the movement
		[Tooltip("the acceleration of the movement")]
		[MMFCondition("AnimateZ", true)]
		public AnimationCurve AnimatePositionCurveZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
        
		[Header("Positions")]
		/// if this is true, the initial position won't be added to init and destination
		[Tooltip("if this is true, the initial position won't be added to init and destination")]
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

		protected Vector3 _newPosition;
		protected RectTransform _rectTransform;
		protected Vector3 _initialPosition;
		protected Vector3 _destinationPosition;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init, we set our initial and destination positions (transform will take precedence over vector3s)
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
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
            
			if (Mode != Modes.ToDestination)
			{
				if (InitialPositionTransform != null)
				{
					InitialPosition = GetPosition(InitialPositionTransform);
				}
				else
				{
					InitialPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + InitialPosition : GetPosition(AnimatePositionTarget.transform);
				}
				if (DestinationPositionTransform != null)
				{
					DestinationPosition = GetPosition(DestinationPositionTransform);
				}
				else
				{
					DestinationPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + DestinationPosition : DestinationPosition;
				}
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
            
			if (isActiveAndEnabled || _hostMMFeedbacks.AutoPlayOnEnable)
			{
				if (DeterminePositionsOnPlay && NormalPlayDirection)
				{
					DeterminePositions();
				}
                
				switch (Mode)
				{
					case Modes.ToDestination:
						_initialPosition = GetPosition(AnimatePositionTarget.transform);
						_destinationPosition = RelativePosition ? _initialPosition + DestinationPosition : DestinationPosition;
						if (DestinationPositionTransform != null)
						{
							_destinationPosition = GetPosition(DestinationPositionTransform);
						}
						_coroutine = StartCoroutine(MoveFromTo(AnimatePositionTarget, _initialPosition, _destinationPosition, FeedbackDuration, AnimatePositionCurve));
						break;
					case Modes.AtoB:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = StartCoroutine(MoveFromTo(AnimatePositionTarget, InitialPosition, DestinationPosition, FeedbackDuration, AnimatePositionCurve));
						break;
					case Modes.AlongCurve:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
						_coroutine = StartCoroutine(MoveAlongCurve(AnimatePositionTarget, InitialPosition, FeedbackDuration, intensityMultiplier));
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

				if (TimeScale == TimeScales.Scaled)
				{
					journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				}
				else
				{
					journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
				}
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
			float newValueX = AnimatePositionCurveX.Evaluate(percent);
			float newValueY = AnimatePositionCurveY.Evaluate(percent);
			float newValueZ = AnimatePositionCurveZ.Evaluate(percent);

			newValueX = MMFeedbacksHelpers.Remap(newValueX, 0f, 1f, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier);
			newValueY = MMFeedbacksHelpers.Remap(newValueY, 0f, 1f, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier);
			newValueZ = MMFeedbacksHelpers.Remap(newValueZ, 0f, 1f, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier);

			_newPosition = initialPosition;

			if (RelativePosition)
			{
				_newPosition.x = AnimateX ? initialPosition.x + newValueX : initialPosition.x;
				_newPosition.y = AnimateY ? initialPosition.y + newValueY : initialPosition.y;
				_newPosition.z = AnimateZ ? initialPosition.z + newValueZ : initialPosition.z;

			}
			else
			{
				_newPosition.x = AnimateX ? newValueX : initialPosition.x;
				_newPosition.y = AnimateY ? newValueY : initialPosition.y;
				_newPosition.z = AnimateZ ? newValueZ : initialPosition.z;
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
		protected virtual IEnumerator MoveFromTo(GameObject movingObject, Vector3 pointA, Vector3 pointB, float duration, AnimationCurve curve = null)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);

				_newPosition = Vector3.LerpUnclamped(pointA, pointB, curve.Evaluate(percent));

				SetPosition(movingObject.transform, _newPosition);

				if (TimeScale == TimeScales.Scaled)
				{
					journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				}
				else
				{
					journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
				}
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
			StopCoroutine(_coroutine);
			_coroutine = null;
		}

		/// <summary>
		/// On disable we reset our coroutine
		/// </summary>
		protected virtual void OnDisable()
		{
			_coroutine = null;
		}
	}
}