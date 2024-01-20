using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you modify the scale of an object on an axis while the other two axis (or only one) get automatically modified to conserve mass
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Transform/Squash and Stretch")]
	[FeedbackHelp("This feedback will let you modify the scale of an object on an axis while the other two axis (or only one) get automatically modified to conserve mass.")]
	public class MMF_SquashAndStretch : MMF_Feedback
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (SquashAndStretchTarget == null); }
		public override string RequiredTargetText { get { return SquashAndStretchTarget != null ? SquashAndStretchTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a SquashAndStretchTarget be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => SquashAndStretchTarget = FindAutomatedTarget<Transform>();
		
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes this feedback can operate on
		public enum Modes { Absolute, Additive, ToDestination }
		/// the various axis on which to apply the squash and stretch
		public enum PossibleAxis { XtoYZ, XtoY, XtoZ, YtoXZ, YtoX, YtoZ, ZtoXZ, ZtoX, ZtoY }
		/// the possible timescales for the animation of the scale
		public enum TimeScales { Scaled, Unscaled }

		[MMFInspectorGroup("Squash & Stretch", true, 54, true)]

		/// the object to animate
		[Tooltip("the object to animate")]
		public Transform SquashAndStretchTarget;
		/// the mode this feedback should operate on
		/// Absolute : follows the curve
		/// Additive : adds to the current scale of the target
		/// ToDestination : sets the scale to the destination target, whatever the current scale is
		[Tooltip("the mode this feedback should operate on" +
		         "Absolute : follows the curve" +
		         "Additive : adds to the current scale of the target" +
		         "ToDestination : sets the scale to the destination target, whatever the current scale is")]
		public Modes Mode = Modes.Absolute;
		public PossibleAxis Axis = PossibleAxis.YtoXZ;
		/// the duration of the animation
		[Tooltip("the duration of the animation")]
		public float AnimateScaleDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("the value to remap the curve's 0 value to")]
		public float RemapCurveZero = 1f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[FormerlySerializedAs("Multiplier")]
		public float RemapCurveOne = 2f;
		/// how much should be added to the curve
		[Tooltip("how much should be added to the curve")]
		public float Offset = 0f;
		/// the curve along which to animate the scale
		[Tooltip("the curve along which to animate the scale")]
		public AnimationCurve AnimateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination scales will be recomputed on every play
		[Tooltip("if this is true, initial and destination scales will be recomputed on every play")]
		public bool DetermineScaleOnPlay = false;
		/// the scale to reach when in ToDestination mode
		[Tooltip("the scale to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationScale = 2f;

		/// the duration of this feedback is the duration of the scale animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }
		public override bool HasRandomness => true;

		protected Vector3 _initialScale;
		protected float _initialAxisScale;
		protected Vector3 _newScale;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (SquashAndStretchTarget != null))
			{
				GetInitialScale();
			}
		}

		/// <summary>
		/// Stores initial scale for future use
		/// </summary>
		protected virtual void GetInitialScale()
		{
			_initialScale = SquashAndStretchTarget.localScale;
		}

		protected virtual void GetAxisScale()
		{
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.XtoY:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.XtoZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.YtoXZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.YtoX:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.YtoZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.ZtoXZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
				case PossibleAxis.ZtoX:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
				case PossibleAxis.ZtoY:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
			}
		}

		/// <summary>
		/// On Play, triggers the scale animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (SquashAndStretchTarget == null)) 
			{
				return;
			}
            
			if (DetermineScaleOnPlay && NormalPlayDirection)
			{
				GetInitialScale();
			}
            
			GetAxisScale();
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (Active || Owner.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(AnimateScale(SquashAndStretchTarget, FeedbackDuration, AnimateCurve, Axis, RemapCurveZero, RemapCurveOne * intensityMultiplier));
				}
				if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(ScaleToDestination());
				}                   
			}
		}

		/// <summary>
		/// An internal coroutine used to scale the target to its destination scale
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ScaleToDestination()
		{
			if (SquashAndStretchTarget == null)
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialScale = SquashAndStretchTarget.localScale;
			_newScale = _initialScale;
			GetAxisScale();
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				float newScale = Mathf.LerpUnclamped(_initialAxisScale, DestinationScale, AnimateCurve.Evaluate(percent) + Offset);
				newScale = MMFeedbacksHelpers.Remap(newScale, 0f, 1f, RemapCurveZero, RemapCurveOne);

				ApplyScale(newScale);
                
				SquashAndStretchTarget.localScale = _newScale;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			ApplyScale(DestinationScale);
			SquashAndStretchTarget.localScale = NormalPlayDirection ? _newScale : _initialScale;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// An internal coroutine used to animate the scale over time
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="duration"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateScale(Transform targetTransform, float duration, AnimationCurve curve, PossibleAxis axis, float remapCurveZero = 0f, float remapCurveOne = 1f)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;
            
			_initialScale = targetTransform.localScale;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
				ComputeAndApplyScale(percent, curve, remapCurveZero, remapCurveOne, targetTransform);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			ComputeAndApplyScale(1f, curve, remapCurveZero, remapCurveOne, targetTransform);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Computes the new scale based on the current percent, and applies it to the transform
		/// </summary>
		/// <param name="percent"></param>
		/// <param name="curve"></param>
		/// <param name="remapCurveZero"></param>
		/// <param name="remapCurveOne"></param>
		/// <param name="targetTransform"></param>
		protected virtual void ComputeAndApplyScale(float percent, AnimationCurve curve, float remapCurveZero, float remapCurveOne, Transform targetTransform)
		{
			float newScale = curve.Evaluate(percent) + Offset;
			newScale = MMFeedbacksHelpers.Remap(newScale, 0f, 1f, remapCurveZero, remapCurveOne);
			if (Mode == Modes.Additive)
			{
				newScale += _initialAxisScale;
			}
			newScale = Mathf.Abs(newScale);
			ApplyScale(newScale);
			targetTransform.localScale = _newScale;
		}

		/// <summary>
		/// Applies the new scale on the selected axis
		/// </summary>
		/// <param name="newScale"></param>
		protected virtual void ApplyScale(float newScale)
		{
			float invertScale = 1 / Mathf.Sqrt(newScale);
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_newScale.x = newScale;
					_newScale.y = invertScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.XtoY:
					_newScale.x = newScale;
					_newScale.y = invertScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.XtoZ:
					_newScale.x = newScale;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoXZ:
					_newScale.x = invertScale;
					_newScale.y = newScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoX:
					_newScale.x = invertScale;
					_newScale.y = newScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.YtoZ:
					_newScale.x = newScale;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.ZtoXZ:
					_newScale.x = invertScale;
					_newScale.y = invertScale;
					_newScale.z = newScale;
					break;
				case PossibleAxis.ZtoX:
					_newScale.x = invertScale;
					_newScale.y = _initialScale.y;
					_newScale.z = newScale;
					break;
				case PossibleAxis.ZtoY:
					_newScale.x = _initialScale.x;
					_newScale.y = invertScale;
					_newScale.z = newScale;
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
			if (Active && (_coroutine != null))
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
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			SquashAndStretchTarget.localScale = _initialScale;
		}
	}
}