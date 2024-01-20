using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.")]
	[FeedbackPath("Transform/Destination")]
	public class MMFeedbackDestinationTransform : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible timescales this feedback can animate on
		public enum TimeScales { Scaled, Unscaled }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		#endif

		[Header("Target to animate")]
		/// the target transform we want to animate properties on
		[Tooltip("the target transform we want to animate properties on")]
		public Transform TargetTransform;
        
		[Header("Origin and destination")]
		/// whether or not we want to force an origin transform. If not, the current position of the target transform will be used as origin instead
		[Tooltip("whether or not we want to force an origin transform. If not, the current position of the target transform will be used as origin instead")]
		public bool ForceOrigin = false;
		/// the transform to use as origin in ForceOrigin mode
		[Tooltip("the transform to use as origin in ForceOrigin mode")]
		[MMFCondition("ForceOrigin", true)] 
		public Transform Origin;
		/// the destination transform whose properties we want to match 
		[Tooltip("the destination transform whose properties we want to match")]
		public Transform Destination;
        
		[Header("Transition")]
		/// the timescale to animate on
		[Tooltip("the timescale to animate on")]
		public TimeScales TimeScale = TimeScales.Scaled;
		/// whether or not we want to force transform properties at the end of the transition
		[Tooltip("whether or not we want to force transform properties at the end of the transition")]
		public bool ForceDestinationOnEnd = false;
		/// a global curve to animate all properties on, unless dedicated ones are specified
		[Tooltip("a global curve to animate all properties on, unless dedicated ones are specified")]
		public AnimationCurve GlobalAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the duration of the transition, in seconds
		[Tooltip("the duration of the transition, in seconds")]
		public float Duration = 0.2f;

		[Header("Axis Locks")]
        
		/// whether or not to animate the X position
		[Tooltip("whether or not to animate the X Position")]
		public bool AnimatePositionX = true;
		/// whether or not to animate the Y position
		[Tooltip("whether or not to animate the Y Position")]
		public bool AnimatePositionY = true;
		/// whether or not to animate the Z position
		[Tooltip("whether or not to animate the Z Position")]
		public bool AnimatePositionZ = true;
		/// whether or not to animate the X rotation
		[Tooltip("whether or not to animate the X rotation")]
		public bool AnimateRotationX = true;
		/// whether or not to animate the Y rotation
		[Tooltip("whether or not to animate the Y rotation")]
		public bool AnimateRotationY = true;
		/// whether or not to animate the Z rotation
		[Tooltip("whether or not to animate the Z rotation")]
		public bool AnimateRotationZ = true;
		/// whether or not to animate the W rotation
		[Tooltip("whether or not to animate the W rotation")]
		public bool AnimateRotationW = true;
		/// whether or not to animate the X scale
		[Tooltip("whether or not to animate the X scale")]
		public bool AnimateScaleX = true;
		/// whether or not to animate the Y scale
		[Tooltip("whether or not to animate the Y scale")]
		public bool AnimateScaleY = true;
		/// whether or not to animate the Z scale
		[Tooltip("whether or not to animate the Z scale")]
		public bool AnimateScaleZ = true;

		[Header("Separate Curves")] 
		/// whether or not to use a separate animation curve to animate the position
		[Tooltip("whether or not to use a separate animation curve to animate the position")]
		public bool SeparatePositionCurve = false;
		/// the curve to use to animate the position on
		[Tooltip("the curve to use to animate the position on")]
		[MMFCondition("SeparatePositionCurve", true)]
		public AnimationCurve AnimatePositionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        
		/// whether or not to use a separate animation curve to animate the rotation
		[Tooltip("whether or not to use a separate animation curve to animate the rotation")]
		public bool SeparateRotationCurve = false;
		/// the curve to use to animate the rotation on
		[Tooltip("the curve to use to animate the rotation on")]
		[MMFCondition("SeparateRotationCurve", true)]
		public AnimationCurve AnimateRotationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        
		/// whether or not to use a separate animation curve to animate the scale
		[Tooltip("whether or not to use a separate animation curve to animate the scale")]
		public bool SeparateScaleCurve = false;
		/// the curve to use to animate the scale on
		[Tooltip("the curve to use to animate the scale on")]
		[MMFCondition("SeparateScaleCurve", true)]
		public AnimationCurve AnimateScaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        
		/// the duration of this feedback is the duration of the movement
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		protected Coroutine _coroutine;
		protected Vector3 _newPosition;
		protected Quaternion _newRotation;
		protected Vector3 _newScale;
		protected Vector3 _pointAPosition;
		protected Vector3 _pointBPosition;
		protected Quaternion _pointARotation;
		protected Quaternion _pointBRotation;
		protected Vector3 _pointAScale;
		protected Vector3 _pointBScale;
		protected AnimationCurve _animationCurve;
        
		/// <summary>
		/// On Play we animate the pos/rotation/scale of the target transform towards its destination
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetTransform == null))
			{
				return;
			}
			_coroutine = StartCoroutine(AnimateToDestination());
		}

		/// <summary>
		/// A coroutine used to animate the pos/rotation/scale of the target transform towards its destination
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AnimateToDestination()
		{
			_pointAPosition = ForceOrigin ? Origin.transform.position : TargetTransform.position;
			_pointBPosition = Destination.transform.position;

			if (!AnimatePositionX) { _pointAPosition.x = TargetTransform.position.x; _pointBPosition.x = _pointAPosition.x; }
			if (!AnimatePositionY) { _pointAPosition.y = TargetTransform.position.y; _pointBPosition.y = _pointAPosition.y; }
			if (!AnimatePositionZ) { _pointAPosition.z = TargetTransform.position.z; _pointBPosition.z = _pointAPosition.z; }
            
			_pointARotation = ForceOrigin ? Origin.transform.rotation : TargetTransform.rotation;
			_pointBRotation = Destination.transform.rotation;
            
			if (!AnimateRotationX) { _pointARotation.x = TargetTransform.rotation.x; _pointBRotation.x = _pointARotation.x; }
			if (!AnimateRotationY) { _pointARotation.y = TargetTransform.rotation.y; _pointBRotation.y = _pointARotation.y; }
			if (!AnimateRotationZ) { _pointARotation.z = TargetTransform.rotation.z; _pointBRotation.z = _pointARotation.z; }
			if (!AnimateRotationW) { _pointARotation.w = TargetTransform.rotation.w; _pointBRotation.w = _pointARotation.w; }

			_pointAScale = ForceOrigin ? Origin.transform.localScale : TargetTransform.localScale;
			_pointBScale = Destination.transform.localScale;
            
			if (!AnimateScaleX) { _pointAScale.x = TargetTransform.localScale.x; _pointBScale.x = _pointAScale.x; }
			if (!AnimateScaleY) { _pointAScale.y = TargetTransform.localScale.y; _pointBScale.y = _pointAScale.y; }
			if (!AnimateScaleZ) { _pointAScale.z = TargetTransform.localScale.z; _pointBScale.z = _pointAScale.z; }

			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : Duration;
			while ((journey >= 0) && (journey <= Duration) && (Duration > 0))
			{
				float percent = Mathf.Clamp01(journey / Duration);

				_animationCurve = SeparatePositionCurve ? AnimatePositionCurve : GlobalAnimationCurve;
				_newPosition = Vector3.LerpUnclamped(_pointAPosition, _pointBPosition, _animationCurve.Evaluate(percent));
                
				_animationCurve = SeparateRotationCurve ? AnimateRotationCurve : GlobalAnimationCurve;
				_newRotation = Quaternion.LerpUnclamped(_pointARotation, _pointBRotation, _animationCurve.Evaluate(percent));
                
				_animationCurve = SeparateScaleCurve ? AnimateScaleCurve : GlobalAnimationCurve;
				_newScale = Vector3.LerpUnclamped(_pointAScale, _pointBScale, _animationCurve.Evaluate(percent));

				TargetTransform.position = _newPosition;
				TargetTransform.rotation = _newRotation;
				TargetTransform.localScale = _newScale;

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
			if (ForceDestinationOnEnd)
			{
				if (NormalPlayDirection)
				{
					TargetTransform.position = _pointBPosition;
					TargetTransform.rotation = _pointBRotation;
					TargetTransform.localScale = _pointBScale;
				}
				else
				{
					TargetTransform.position = _pointAPosition;
					TargetTransform.rotation = _pointARotation;
					TargetTransform.localScale = _pointAScale;
				}    
			}
			IsPlaying = false;
			_coroutine = null;
			yield break;
		}

		/// <summary>
		/// On Stop we stop our coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
            
			if ((TargetTransform != null) && (_coroutine != null))
			{
				StopCoroutine(_coroutine);
			}
		}
	}    
}