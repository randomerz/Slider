using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to a UI rectangle and it'll act as a detection zone for a follower joystick.
	/// Note that this component extends the MMTouchJoystick class so you don't need to add another joystick to it. It's both the detection zone and the stick itself.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Controls/MMTouchFollowerJoystick")]
	public class MMTouchFollowerJoystick : MMTouchJoystick
	{
		[MMInspectorGroup("Follower Joystick", true, 23)]
		/// the canvas group to use as the joystick's knob - the part that moves under your thumb
		[Tooltip("the canvas group to use as the joystick's knob - the part that moves under your thumb")]
		public CanvasGroup KnobCanvasGroup;
		/// the canvas group to use as the joystick's background
		[Tooltip("the canvas group to use as the joystick's background")]
		public CanvasGroup BackgroundCanvasGroup;
		/// if this is true, the joystick will return back to its initial position when released
		[Tooltip("if this is true, the joystick will return back to its initial position when released")]
		public bool ResetPositionToInitialOnRelease = false;
		/// if this is true, the background will follow its target with interpolation, otherwise it'll be instant movement
		[Tooltip("if this is true, the background will follow its target with interpolation, otherwise it'll be instant movement")]
		public bool InterpolateFollowMovement = false;
		/// if in interpolate mode, this defines the speed at which the backgrounds follows the knob
		[Tooltip("if in interpolate mode, this defines the speed at which the backgrounds follows the knob")]
		[MMCondition("InterpolateFollowMovement", true)]
		public float InterpolateFollowMovementSpeed = 0.3f;
		/// whether or not to add a spring to the interpolation of the background movement
		[Tooltip("whether or not to add a spring to the interpolation of the background movement")]
		[MMCondition("InterpolateFollowMovement", true)]
		public bool SpringFollowInterpolation = false;
		/// when in SpringFollowInterpolation mode, the amount of damping to apply to the spring
		[Tooltip("when in SpringFollowInterpolation mode, the amount of damping to apply to the spring")]
		[MMCondition("SpringFollowInterpolation", true)]
		public float SpringDamping = 0.6f;
		/// when in SpringFollowInterpolation mode, the frequency to apply to the spring
		[Tooltip("when in SpringFollowInterpolation mode, the frequency to apply to the spring")]
		[MMCondition("SpringFollowInterpolation", true)]
		public float SpringFrequency = 4f;
		
		[MMInspectorGroup("Background Constraints", true, 24)]
		/// if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas
		[Tooltip("if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas")]
		public bool ShouldConstrainBackground = true;
		/// the rect to consider as a background constraint zone, if left empty, will be auto created
		[Tooltip("the rect to consider as a background constraint zone, if left empty, will be auto created")]
		public RectTransform BackgroundConstraintRectTransform;
		/// the left padding to apply to the background constraint
		[Tooltip("the left padding to apply to the background constraint")]
		public float BackgroundConstraintPaddingLeft;
		/// the right padding to apply to the background constraint
		[Tooltip("the right padding to apply to the background constraint")]
		public float BackgroundConstraintPaddingRight;
		/// the top padding to apply to the background constraint
		[Tooltip("the top padding to apply to the background constraint")]
		public float BackgroundConstraintPaddingTop;
		/// the bottom padding to apply to the background constraint
		[Tooltip("the bottom padding to apply to the background constraint")]
		public float BackgroundConstraintPaddingBottom;
		
		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected RectTransform _rectTransform;
		protected RectTransform _backgroundRectTransform;
		protected Vector3[] _innerRectCorners = new Vector3[4];
		protected Vector3 _newBackgroundPosition;
		protected Vector3 _backgroundPositionTarget;
		protected Vector3 _innerRectTransformBottomLeft;
		protected Vector3 _innerRectTransformTopLeft;
		protected Vector3 _innerRectTransformTopRight;
		protected Vector3 _innerRectTransformBottomRight;
		protected Vector3 _springVelocity;

		/// <summary>
		/// On Start, we instantiate our joystick's image if there's one
		/// </summary>
		protected override void Start()
		{
			base.Start();

			// we store the detection zone's initial position
			_rectTransform = GetComponent<RectTransform>();
			_backgroundRectTransform = BackgroundCanvasGroup.GetComponent<RectTransform>();
			_initialPosition = _backgroundRectTransform.position;
			_backgroundPositionTarget = _initialPosition;
			
			CreateInnerRect();
		}

		/// <summary>
		/// On initialize, we set our knob transform
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			SetKnobTransform(KnobCanvasGroup.transform);
			_canvasGroup = KnobCanvasGroup;
			_initialOpacity = _canvasGroup.alpha;
		}
		
		/// <summary>
		/// On update, we handle movement interpolation
		/// </summary>
		protected override void Update()
		{
			base.Update();
			HandleMovementInterpolation();
		}

		/// <summary>
		/// Handles the movement of the background relative to the knob
		/// </summary>
		protected virtual void HandleMovementInterpolation()
		{
			if (!InterpolateFollowMovement)
			{
				BackgroundCanvasGroup.transform.position = _backgroundPositionTarget;	
				return;
			}

			if (SpringFollowInterpolation)
			{
				_newBackgroundPosition = BackgroundCanvasGroup.transform.position;
				MMMaths.Spring(ref _newBackgroundPosition, _backgroundPositionTarget, ref _springVelocity, SpringDamping, SpringFrequency, InterpolateFollowMovementSpeed, Time.unscaledDeltaTime);
				BackgroundCanvasGroup.transform.position = _newBackgroundPosition;
			}
			else
			{
				BackgroundCanvasGroup.transform.position = MMMaths.Lerp(BackgroundCanvasGroup.transform.position, _backgroundPositionTarget, InterpolateFollowMovementSpeed, Time.unscaledDeltaTime);	
			}
		}

		/// <summary>
		/// Creates a constraining inner rect
		/// </summary>
		protected virtual void CreateInnerRect()
		{
			if (!ShouldConstrainBackground)
			{
				return;
			}
			
			// we create an inner rect if one wasn't provided
			if (BackgroundConstraintRectTransform == null)
			{
				GameObject innerRect = new GameObject();
				innerRect.transform.SetParent(this.transform);
				innerRect.name = "BackgroundConstraintRectTransform";
				BackgroundConstraintRectTransform = innerRect.AddComponent<RectTransform>();
				BackgroundConstraintRectTransform.anchorMin = _rectTransform.anchorMin;
				BackgroundConstraintRectTransform.anchorMax = _rectTransform.anchorMax;
				BackgroundConstraintRectTransform.position = _rectTransform.position;
				BackgroundConstraintRectTransform.localScale = _rectTransform.localScale;
				BackgroundConstraintRectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x - _backgroundRectTransform.sizeDelta.y, _rectTransform.sizeDelta.y - _backgroundRectTransform.sizeDelta.y);	
			}

			// we apply the padding
			BackgroundConstraintRectTransform.offsetMin += new Vector2(BackgroundConstraintPaddingLeft, BackgroundConstraintPaddingBottom);
			BackgroundConstraintRectTransform.offsetMax -= new Vector2(BackgroundConstraintPaddingRight, BackgroundConstraintPaddingTop);
			
			// we store our corners
			BackgroundConstraintRectTransform.GetWorldCorners(_innerRectCorners);
			_innerRectTransformBottomLeft = _innerRectCorners[0];
			_innerRectTransformTopLeft = _innerRectCorners[1];
			_innerRectTransformTopRight = _innerRectCorners[2];
			_innerRectTransformBottomRight = _innerRectCorners[3];
		}
		
		/// <summary>
		/// When the zone is pressed, we move our joystick accordingly
		/// </summary>
		/// <param name="data">Data.</param>
		public override void OnPointerDown(PointerEventData data)
		{
			base.OnPointerDown(data);
			
			_newPosition = ConvertToWorld(data.position);
			_newPosition.z = this.transform.position.z;
			
			// we define a new neutral position
			
			_backgroundPositionTarget = _newPosition;
			ConstrainBackground();
			SetNeutralPosition(BackgroundCanvasGroup.transform.position);
			_knobTransform.position = _newPosition;
			
			ComputeJoystickValue();
		}

		/// <summary>
		/// On drag, we adjust our target and constrain our background
		/// </summary>
		/// <param name="eventData"></param>
		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);

			float distance = Vector2.Distance(_knobTransform.position, BackgroundCanvasGroup.transform.position); 
			if (distance >= ComputedMaxRange)
			{
				_backgroundPositionTarget = BackgroundCanvasGroup.transform.position +
				                            (_knobTransform.position - BackgroundCanvasGroup.transform.position).normalized * (distance - ComputedMaxRange);
			}

			ConstrainBackground();
			ComputeJoystickValue();
		}

		/// <summary>
		/// Determines the value of the joystick by computing the 
		/// </summary>
		protected virtual void ComputeJoystickValue()
		{
			float distance = Vector2.Distance(_knobTransform.position, BackgroundCanvasGroup.transform.position);
			if (distance <= ComputedMaxRange)
			{
				RawValue.x = EvaluateInputValue(_knobTransform.position.x - BackgroundCanvasGroup.transform.position.x);
				RawValue.y = EvaluateInputValue(_knobTransform.position.y - BackgroundCanvasGroup.transform.position.y);	
			}
			else
			{
				float vectorPosition = _knobTransform.position.x - BackgroundCanvasGroup.transform.position.x;
				RawValue.x = Mathf.InverseLerp(0, distance, Mathf.Abs(vectorPosition)) * Mathf.Sign(vectorPosition);
				vectorPosition = _knobTransform.position.y - BackgroundCanvasGroup.transform.position.y;
				RawValue.y = Mathf.InverseLerp(0, distance, Mathf.Abs(vectorPosition)) * Mathf.Sign(vectorPosition);
			}
		}

		/// <summary>
		/// Clamps the background inside the inner rect
		/// </summary>
		protected virtual void ConstrainBackground()
		{
			if (!ShouldConstrainBackground)
			{
				return;
			}
			_newBackgroundPosition = _backgroundPositionTarget;
			_newBackgroundPosition.x = Mathf.Clamp(_newBackgroundPosition.x , _innerRectTransformTopLeft.x, _innerRectTransformTopRight.x);
			_newBackgroundPosition.y = Mathf.Clamp(_newBackgroundPosition.y , _innerRectTransformBottomLeft.y, _innerRectTransformTopLeft.y);
			_backgroundPositionTarget = _newBackgroundPosition;
		}

		/// <summary>
		/// On pointer up we reset our joystick
		/// </summary>
		/// <param name="data"></param>
		public override void OnPointerUp(PointerEventData data)
		{
			base.OnPointerUp(data);
			
			ResetJoystick();
			_knobTransform.position = _backgroundPositionTarget;

			if (ResetPositionToInitialOnRelease)
			{
				_backgroundPositionTarget = _initialPosition;
				_knobTransform.position = _initialPosition;
			}
		}

		/// <summary>
		/// We don't clamp the stick anymore
		/// </summary>
		protected override void ClampToBounds()
		{
			_newTargetPosition = _newTargetPosition - _neutralPosition;
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Draws gizmos to show the constraining box' corners 
		/// </summary>
		protected override void OnDrawGizmos()
		{
			if (!DrawGizmos)
			{
				return;
			}
			
			// Draws max range
			Handles.color = MMColors.Orange;
			if (!Application.isPlaying)
			{
				if (KnobCanvasGroup != null)
				{
					Handles.DrawWireDisc(KnobCanvasGroup.transform.position, Vector3.forward, ComputedMaxRange);	
				}
				else
				{
					Handles.DrawWireDisc(this.transform.position, Vector3.forward, ComputedMaxRange);	
				}
			}
			else
			{
				Handles.DrawWireDisc(_backgroundRectTransform.position, Vector3.forward, ComputedMaxRange);
			}
			
			// Draws corners
			if (BackgroundConstraintRectTransform != null)
			{
				float gizmoSize = 0.3f;
				MMDebug.DrawGizmoPoint(_innerRectTransformBottomLeft, Color.cyan, gizmoSize);
				MMDebug.DrawGizmoPoint(_innerRectTransformTopLeft, Color.cyan, gizmoSize);
				MMDebug.DrawGizmoPoint(_innerRectTransformTopRight, Color.cyan, gizmoSize);
				MMDebug.DrawGizmoPoint(_innerRectTransformBottomRight, Color.cyan, gizmoSize);
			}
		}
		#endif
	}
}