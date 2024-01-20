using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.")]
	[FeedbackPath("Transform/Destination")]
	public class MMF_DestinationTransform : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible timescales this feedback can animate on
		public enum TimeScales { Scaled, Unscaled }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetTransform == null) || (Destination == null); }
		public override string RequiredTargetText { get { return TargetTransform != null ? TargetTransform.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTransform and a Destination be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTransform = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Target to animate", true, 61, true)]
		/// the target transform we want to animate properties on
		[Tooltip("the target transform we want to animate properties on")]
		public Transform TargetTransform;
        
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
        
		[MMFInspectorGroup("Transition", true, 63)]
		/// a global curve to animate all properties on, unless dedicated ones are specified
		[Tooltip("a global curve to animate all properties on, unless dedicated ones are specified")]
		public MMTweenType GlobalAnimationTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the duration of the transition, in seconds
		[Tooltip("the duration of the transition, in seconds")]
		public float Duration = 0.2f;

		[MMFInspectorGroup("Axis Locks", true, 64)]
        
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

		[MMFInspectorGroup("Separate Curves", true, 65)]
		/// whether or not to use a separate animation curve to animate the position
		[Tooltip("whether or not to use a separate animation curve to animate the position")]
		public bool SeparatePositionCurve = false;
		/// the curve to use to animate the position on
		[Tooltip("the curve to use to animate the position on")]
		[MMFCondition("SeparatePositionCurve", true)]
		public MMTweenType AnimatePositionTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        
		/// whether or not to use a separate animation curve to animate the rotation
		[Tooltip("whether or not to use a separate animation curve to animate the rotation")]
		public bool SeparateRotationCurve = false;
		/// the curve to use to animate the rotation on
		[Tooltip("the curve to use to animate the rotation on")]
		[MMFCondition("SeparateRotationCurve", true)]
		public MMTweenType AnimateRotationTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        
		/// whether or not to use a separate animation curve to animate the scale
		[Tooltip("whether or not to use a separate animation curve to animate the scale")]
		public bool SeparateScaleCurve = false;
		/// the curve to use to animate the scale on
		[Tooltip("the curve to use to animate the scale on")]
		[MMFCondition("SeparateScaleCurve", true)]
		public MMTweenType AnimateScaleTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
        
		/// the duration of this feedback is the duration of the movement
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// a global curve to animate all properties on, unless dedicated ones are specified
		[HideInInspector] public AnimationCurve GlobalAnimationCurve = null;
		/// the curve to use to animate the position on
		[HideInInspector] public AnimationCurve AnimateScaleCurve = null;
		/// the curve to use to animate the rotation on
		[HideInInspector] public AnimationCurve AnimatePositionCurve = null;
		/// the curve to use to animate the scale on
		[HideInInspector] public AnimationCurve AnimateRotationCurve = null;
		
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
		protected MMTweenType _animationTweenType;

		protected Vector3 _initialPosition;
		protected Vector3 _initialScale;
		protected Quaternion _initialRotation;
        
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
			_coroutine = Owner.StartCoroutine(AnimateToDestination());
		}

		/// <summary>
		/// A coroutine used to animate the pos/rotation/scale of the target transform towards its destination
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AnimateToDestination()
		{
			_initialPosition = TargetTransform.position;
			_initialRotation = TargetTransform.rotation;
			_initialScale = TargetTransform.localScale;
			
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
				ChangeTransformValues(percent);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			// set final position
			ChangeTransformValues(1f);
			
			IsPlaying = false;
			_coroutine = null;
			yield break;
		}

		/// <summary>
		/// Computes the new position, rotation and scale for our transform, and applies it to the transform
		/// </summary>
		/// <param name="percent"></param>
		protected virtual void ChangeTransformValues(float percent)
		{
			_animationTweenType = SeparatePositionCurve ? AnimatePositionTween : GlobalAnimationTween;
			_newPosition = Vector3.LerpUnclamped(_pointAPosition, _pointBPosition, _animationTweenType.Evaluate(percent));
                
			_animationTweenType = SeparateRotationCurve ? AnimateRotationTween : GlobalAnimationTween;
			_newRotation = Quaternion.LerpUnclamped(_pointARotation, _pointBRotation, _animationTweenType.Evaluate(percent));
                
			_animationTweenType = SeparateScaleCurve ? AnimateScaleTween : GlobalAnimationTween;
			_newScale = Vector3.LerpUnclamped(_pointAScale, _pointBScale, _animationTweenType.Evaluate(percent));
			
			TargetTransform.position = _newPosition;
			TargetTransform.rotation = _newRotation;
			TargetTransform.localScale = _newScale;
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
				Owner.StopCoroutine(_coroutine);
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
			TargetTransform.position = _initialPosition;
			TargetTransform.rotation = _initialRotation;
			TargetTransform.localScale = _initialScale;
		}
		
		/// <summary>
		/// On Validate, we migrate our deprecated animation curves to our tween types if needed
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(GlobalAnimationCurve, GlobalAnimationTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurve, AnimatePositionTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationCurve, AnimateRotationTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateScaleCurve, AnimateScaleTween, Owner);
		}
	}    
}