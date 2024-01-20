using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will animate the scale of the target object over time when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Transform/Scale")]
	[FeedbackHelp("This feedback will animate the target's scale on the 3 specified animation curves, for the specified duration (in seconds). You can apply a multiplier, that will multiply each animation curve value.")]
	public class MMFeedbackScale : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes this feedback can operate on
		public enum Modes { Absolute, Additive, ToDestination }
		/// the possible timescales for the animation of the scale
		public enum TimeScales { Scaled, Unscaled }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		#endif

		[Header("Scale")]
		/// the mode this feedback should operate on
		/// Absolute : follows the curve
		/// Additive : adds to the current scale of the target
		/// ToDestination : sets the scale to the destination target, whatever the current scale is
		[Tooltip("the mode this feedback should operate on" +
		         "Absolute : follows the curve" +
		         "Additive : adds to the current scale of the target" +
		         "ToDestination : sets the scale to the destination target, whatever the current scale is")]
		public Modes Mode = Modes.Absolute;
		/// whether this feedback should play in scaled or unscaled time
		[Tooltip("whether this feedback should play in scaled or unscaled time")]
		public TimeScales TimeScale = TimeScales.Scaled;
		/// the object to animate
		[Tooltip("the object to animate")]
		public Transform AnimateScaleTarget;
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
		/// if this is true, should animate the X scale value
		[Tooltip("if this is true, should animate the X scale value")]
		public bool AnimateX = true;
		/// the x scale animation definition
		[Tooltip("the x scale animation definition")]
		[MMFCondition("AnimateX", true)]
		public AnimationCurve AnimateScaleX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));
		/// if this is true, should animate the Y scale value
		[Tooltip("if this is true, should animate the Y scale value")]
		public bool AnimateY = true;
		/// the y scale animation definition
		[Tooltip("the y scale animation definition")]
		[MMFCondition("AnimateY", true)]
		public AnimationCurve AnimateScaleY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));
		/// if this is true, should animate the z scale value
		[Tooltip("if this is true, should animate the z scale value")]
		public bool AnimateZ = true;
		/// the z scale animation definition
		[Tooltip("the z scale animation definition")]
		[MMFCondition("AnimateZ", true)]
		public AnimationCurve AnimateScaleZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination scales will be recomputed on every play
		[Tooltip("if this is true, initial and destination scales will be recomputed on every play")]
		public bool DetermineScaleOnPlay = false;

		[Header("To Destination")]
		/// the scale to reach when in ToDestination mode
		[Tooltip("the scale to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector3 DestinationScale = new Vector3(0.5f, 0.5f, 0.5f);

		/// the duration of this feedback is the duration of the scale animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }

		protected Vector3 _initialScale;
		protected Vector3 _newScale;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateScaleTarget != null))
			{
				GetInitialScale();
			}
		}

		/// <summary>
		/// Stores initial scale for future use
		/// </summary>
		protected virtual void GetInitialScale()
		{
			_initialScale = AnimateScaleTarget.localScale;
		}

		/// <summary>
		/// On Play, triggers the scale animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateScaleTarget == null))
			{
				return;
			}
            
			if (DetermineScaleOnPlay && NormalPlayDirection)
			{
				GetInitialScale();
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			if (isActiveAndEnabled || _hostMMFeedbacks.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(AnimateScale(AnimateScaleTarget, Vector3.zero, FeedbackDuration, AnimateScaleX, AnimateScaleY, AnimateScaleZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ScaleToDestination());
				}                   
			}
		}

		/// <summary>
		/// An internal coroutine used to scale the target to its destination scale
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ScaleToDestination()
		{
			if (AnimateScaleTarget == null)
			{
				yield break;
			}

			if ((AnimateScaleX == null) || (AnimateScaleY == null) || (AnimateScaleZ == null))
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialScale = AnimateScaleTarget.localScale;
			_newScale = _initialScale;
			IsPlaying = true;

			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				if (AnimateX)
				{
					_newScale.x = Mathf.LerpUnclamped(_initialScale.x, DestinationScale.x, AnimateScaleX.Evaluate(percent) + Offset);
					_newScale.x = MMFeedbacksHelpers.Remap(_newScale.x, 0f, 1f, RemapCurveZero, RemapCurveOne);    
				}

				if (AnimateY)
				{
					_newScale.y = Mathf.LerpUnclamped(_initialScale.y, DestinationScale.y, AnimateScaleY.Evaluate(percent) + Offset);
					_newScale.y = MMFeedbacksHelpers.Remap(_newScale.y, 0f, 1f, RemapCurveZero, RemapCurveOne);    
				}

				if (AnimateZ)
				{
					_newScale.z = Mathf.LerpUnclamped(_initialScale.z, DestinationScale.z, AnimateScaleZ.Evaluate(percent) + Offset);
					_newScale.z = MMFeedbacksHelpers.Remap(_newScale.z, 0f, 1f, RemapCurveZero, RemapCurveOne);    
				}
                
				AnimateScaleTarget.localScale = _newScale;

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

			AnimateScaleTarget.localScale = NormalPlayDirection ? DestinationScale : _initialScale;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// An internal coroutine used to animate the scale over time
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="vector"></param>
		/// <param name="duration"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateScale(Transform targetTransform, Vector3 vector, float duration, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float remapCurveZero = 0f, float remapCurveOne = 1f)
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
            
			_initialScale = targetTransform.localScale;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				vector = Vector3.zero;
				float percent = Mathf.Clamp01(journey / duration);

				if (AnimateX)
				{
					vector.x = AnimateX ? curveX.Evaluate(percent) + Offset : targetTransform.localScale.x;
					vector.x = MMFeedbacksHelpers.Remap(vector.x, 0f, 1f, remapCurveZero, remapCurveOne);
					if (Mode == Modes.Additive)
					{
						vector.x += _initialScale.x;
					}
				}
				else
				{
					vector.x = targetTransform.localScale.x;
				}

				if (AnimateY)
				{
					vector.y = AnimateY ? curveY.Evaluate(percent) + Offset : targetTransform.localScale.y;
					vector.y = MMFeedbacksHelpers.Remap(vector.y, 0f, 1f, remapCurveZero, remapCurveOne);    
					if (Mode == Modes.Additive)
					{
						vector.y += _initialScale.y;
					}
				}
				else 
				{
					vector.y = targetTransform.localScale.y;
				}

				if (AnimateZ)
				{
					vector.z = AnimateZ ? curveZ.Evaluate(percent) + Offset : targetTransform.localScale.z;
					vector.z = MMFeedbacksHelpers.Remap(vector.z, 0f, 1f, remapCurveZero, remapCurveOne);    
					if (Mode == Modes.Additive)
					{
						vector.z += _initialScale.z;
					}
				}
				else 
				{
					vector.z = targetTransform.localScale.z;
				}
				targetTransform.localScale = vector;

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
            
			vector = Vector3.zero;

			if (AnimateX)
			{
				vector.x = AnimateX ? curveX.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.x;
				vector.x = MMFeedbacksHelpers.Remap(vector.x, 0f, 1f, remapCurveZero, remapCurveOne);
				if (Mode == Modes.Additive)
				{
					vector.x += _initialScale.x;
				}
			}
			else 
			{
				vector.x = targetTransform.localScale.x;
			}

			if (AnimateY)
			{
				vector.y = AnimateY ? curveY.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.y;
				vector.y = MMFeedbacksHelpers.Remap(vector.y, 0f, 1f, remapCurveZero, remapCurveOne);
				if (Mode == Modes.Additive)
				{
					vector.y += _initialScale.y;
				}
			}
			else 
			{
				vector.y = targetTransform.localScale.y;
			}

			if (AnimateZ)
			{
				vector.z = AnimateZ ? curveZ.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.z;
				vector.z = MMFeedbacksHelpers.Remap(vector.z, 0f, 1f, remapCurveZero, remapCurveOne);    
				if (Mode == Modes.Additive)
				{
					vector.z += _initialScale.z;
				}
			}
			else 
			{
				vector.z = targetTransform.localScale.z;
			}
            
			targetTransform.localScale = vector;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
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
            
			StopCoroutine(_coroutine);
			IsPlaying = false;
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