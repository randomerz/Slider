using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A class used to handle the movement and behaviour of floating texts, usually used to display damage text.
	/// This is designed to be spawned by a MMFloatingTextSpawner, not used on its own.
	/// It also requires a specific hierarchy. You'll find examples of it in the MMTools/Tools/MMFloatingText/Prefabs folder
	/// </summary>
	public class MMFloatingText : MonoBehaviour
	{
		[Header("Bindings")]

		/// the part of the prefab that we'll move
		[Tooltip("the part of the prefab that we'll move")]
		public Transform MovingPart;
		/// the part of the prefab that we'll rotate to face the target camera
		[Tooltip("the part of the prefab that we'll rotate to face the target camera")]
		public Transform Billboard;
		/// the TextMesh used to display the value
		[Tooltip("the TextMesh used to display the value")]
		public TextMesh TargetTextMesh;
        
		[Header("Debug")]

		/// the direction of this floating text, used for debug only
		[Tooltip("the direction of this floating text, used for debug only")]
		[MMReadOnly]
		public Vector3 Direction = Vector3.up;

		protected bool _useUnscaledTime = false;
		public virtual float GetTime() { return (_useUnscaledTime) ? Time.unscaledTime : Time.time; }
		public virtual float GetDeltaTime() { return _useUnscaledTime ? Time.unscaledDeltaTime : Time.unscaledTime; }
       
		protected float _startedAt;
		protected float _lifetime;
		protected Vector3 _newPosition;
		protected Color _initialTextColor;
		protected bool _animateMovement;
		protected bool _animateX;
		protected AnimationCurve _animateXCurve;
		protected float _remapXZero;
		protected float _remapXOne;
		protected bool _animateY;
		protected AnimationCurve _animateYCurve;
		protected float _remapYZero;
		protected float _remapYOne;
		protected bool _animateZ;
		protected AnimationCurve _animateZCurve;
		protected float _remapZZero;
		protected float _remapZOne;
		protected MMFloatingTextSpawner.AlignmentModes _alignmentMode;
		protected Vector3 _fixedAlignment;
		protected Vector3 _movementDirection;
		protected Vector3 _movingPartPositionLastFrame;
		protected bool _alwaysFaceCamera;
		protected Camera _targetCamera;
		protected Quaternion _targetCameraRotation;
		protected bool _animateOpacity;
		protected AnimationCurve _animateOpacityCurve;
		protected float _remapOpacityZero;
		protected float _remapOpacityOne;
		protected bool _animateScale;
		protected AnimationCurve _animateScaleCurve;
		protected float _remapScaleZero;
		protected float _remapScaleOne;
		protected bool _animateColor;
		protected Gradient _animateColorGradient;
		protected Vector3 _newScale;
		protected Color _newColor;

		protected float _elapsedTime;
		protected float _remappedTime;

		/// <summary>
		/// On enable, we initialize our floating text
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization();
		}

		/// <summary>
		/// Changes whether or not this floating text should use unscaled time
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetUseUnscaledTime(bool status, bool resetStartedAt)
		{
			_useUnscaledTime = status;
			if (resetStartedAt)
			{
				_startedAt = GetTime();    
			}
		}

		/// <summary>
		/// Stores start time and initial color
		/// </summary>
		protected virtual void Initialization()
		{
			_startedAt = GetTime();
			if (TargetTextMesh != null)
			{
				_initialTextColor = TargetTextMesh.color;
			}            
		}

		/// <summary>
		/// On Update we move our text
		/// </summary>
		protected virtual void Update()
		{
			UpdateFloatingText();
		}

		/// <summary>
		/// Handles the text's life cycle, movement, scale, color, opacity, alignment and billboard
		/// </summary>
		protected virtual void UpdateFloatingText()
		{
            
			_elapsedTime = GetTime() - _startedAt;
			_remappedTime = MMMaths.Remap(_elapsedTime, 0f, _lifetime, 0f, 1f);
            
			// lifetime
			if (_elapsedTime > _lifetime)
			{
				TurnOff();
			}

			HandleMovement();
			HandleColor();
			HandleOpacity();
			HandleScale();
			HandleAlignment();            
			HandleBillboard();
		}

		/// <summary>
		/// Moves the text along the specified curves
		/// </summary>
		protected virtual void HandleMovement()
		{
			// position movement
			if (_animateMovement)
			{
				this.transform.up = Direction;

				_newPosition.x = _animateX ? MMMaths.Remap(_animateXCurve.Evaluate(_remappedTime), 0f, 1, _remapXZero, _remapXOne) : 0f;
				_newPosition.y = _animateY ? MMMaths.Remap(_animateYCurve.Evaluate(_remappedTime), 0f, 1, _remapYZero, _remapYOne) : 0f;
				_newPosition.z = _animateZ ? MMMaths.Remap(_animateZCurve.Evaluate(_remappedTime), 0f, 1, _remapZZero, _remapZOne) : 0f;

				// we move the moving part
				MovingPart.transform.localPosition = _newPosition;

				// we store the last position
				if (Vector3.Distance(_movingPartPositionLastFrame, MovingPart.position) > 0.5f)
				{
					_movingPartPositionLastFrame = MovingPart.position;
				}
			}
		}

		/// <summary>
		/// Animates the text's color over the specified gradient
		/// </summary>
		protected virtual void HandleColor()
		{
			if (_animateColor)
			{
				_newColor = _animateColorGradient.Evaluate(_remappedTime);
				SetColor(_newColor);
			}
		}

		/// <summary>
		/// Animates the text's opacity over the specified curve
		/// </summary>
		protected virtual void HandleOpacity()
		{
			if (_animateOpacity)
			{
				float newOpacity = MMMaths.Remap(_animateOpacityCurve.Evaluate(_remappedTime), 0f, 1f, _remapOpacityZero, _remapOpacityOne);
				SetOpacity(newOpacity);
			}
		}

		/// <summary>
		/// Animates the text's scale over the specified curve
		/// </summary>
		protected virtual void HandleScale()
		{
			if (_animateScale)
			{
				_newScale = Vector3.one * MMMaths.Remap(_animateScaleCurve.Evaluate(_remappedTime), 0f, 1f, _remapScaleZero, _remapScaleOne);
				MovingPart.transform.localScale = _newScale;
			}
		}

		/// <summary>
		/// Handles text rotation to match either a fixed alignment, the initial direction or the movement's direction
		/// </summary>
		protected virtual void HandleAlignment()
		{
			if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.Fixed)
			{
				MovingPart.transform.up = _fixedAlignment;
			}
			else if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.MatchInitialDirection)
			{
				MovingPart.transform.up = this.transform.up;
			}
			else if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.MatchMovementDirection)
			{
				_movementDirection = MovingPart.position - _movingPartPositionLastFrame;
				MovingPart.transform.up = _movementDirection.normalized;
			}
		}

		/// <summary>
		/// Forces the text to face the camera
		/// </summary>
		protected virtual void HandleBillboard()
		{
			if (_alwaysFaceCamera)
			{
				_targetCameraRotation = _targetCamera.transform.rotation;
				Billboard.transform.LookAt(MovingPart.transform.position + _targetCameraRotation * Vector3.forward, _targetCameraRotation * MovingPart.up);
			}
		}

		/// <summary>
		/// Called by the spawner, sets all required variables
		/// </summary>
		/// <param name="value"></param>
		/// <param name="lifetime"></param>
		/// <param name="direction"></param>
		/// <param name="animateMovement"></param>
		/// <param name="alignmentMode"></param>
		/// <param name="fixedAlignment"></param>
		/// <param name="alwaysFaceCamera"></param>
		/// <param name="targetCamera"></param>
		/// <param name="animateX"></param>
		/// <param name="animateXCurve"></param>
		/// <param name="remapXZero"></param>
		/// <param name="remapXOne"></param>
		/// <param name="animateY"></param>
		/// <param name="animateYCurve"></param>
		/// <param name="remapYZero"></param>
		/// <param name="remapYOne"></param>
		/// <param name="animateZ"></param>
		/// <param name="animateZCurve"></param>
		/// <param name="remapZZero"></param>
		/// <param name="remapZOne"></param>
		/// <param name="animateOpacity"></param>
		/// <param name="animateOpacityCurve"></param>
		/// <param name="remapOpacityZero"></param>
		/// <param name="remapOpacityOne"></param>
		/// <param name="animateScale"></param>
		/// <param name="animateScaleCurve"></param>
		/// <param name="remapScaleZero"></param>
		/// <param name="remapScaleOne"></param>
		/// <param name="animateColor"></param>
		/// <param name="animateColorGradient"></param>
		public virtual void SetProperties(string value, float lifetime, Vector3 direction, bool animateMovement, 
			MMFloatingTextSpawner.AlignmentModes alignmentMode, Vector3 fixedAlignment,
			bool alwaysFaceCamera, Camera targetCamera,
			bool animateX, AnimationCurve animateXCurve, float remapXZero, float remapXOne,
			bool animateY, AnimationCurve animateYCurve, float remapYZero, float remapYOne,
			bool animateZ, AnimationCurve animateZCurve, float remapZZero, float remapZOne,
			bool animateOpacity, AnimationCurve animateOpacityCurve, float remapOpacityZero, float remapOpacityOne,
			bool animateScale, AnimationCurve animateScaleCurve, float remapScaleZero, float remapScaleOne,
			bool animateColor, Gradient animateColorGradient)
		{
			SetText(value);
			_lifetime = lifetime;
			Direction = direction;
			_animateMovement = animateMovement;
			_animateX =  animateX;
			_animateXCurve =  animateXCurve;
			_remapXZero =  remapXZero;
			_remapXOne =  remapXOne;
			_animateY =  animateY;
			_animateYCurve =  animateYCurve;
			_remapYZero =  remapYZero;
			_remapYOne =  remapYOne;
			_animateZ =  animateZ;
			_animateZCurve =  animateZCurve;
			_remapZZero =  remapZZero;
			_remapZOne =  remapZOne;
			_alignmentMode = alignmentMode;
			_fixedAlignment = fixedAlignment;
			_alwaysFaceCamera = alwaysFaceCamera;
			_targetCamera = targetCamera;
			_animateOpacity = animateOpacity;
			_animateOpacityCurve = animateOpacityCurve;
			_remapOpacityZero = remapOpacityZero;
			_remapOpacityOne = remapOpacityOne;
			_animateScale = animateScale;
			_animateScaleCurve = animateScaleCurve;
			_remapScaleZero = remapScaleZero;
			_remapScaleOne = remapScaleOne;
			_animateColor = animateColor;
			_animateColorGradient = animateColorGradient;
			UpdateFloatingText();
		}

		/// <summary>
		/// Resets this text's position
		/// </summary>
		public virtual void ResetPosition()
		{
			if (_animateMovement)
			{
				MovingPart.transform.localPosition = Vector3.zero;    
			}
			_movingPartPositionLastFrame = MovingPart.position - Direction;
		}
        
		/// <summary>
		/// Sets the target mesh's text value
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetText(string newValue)
		{
			TargetTextMesh.text = newValue;
		}

		/// <summary>
		/// Sets the color of the target text
		/// </summary>
		/// <param name="newColor"></param>
		public virtual void SetColor(Color newColor)
		{
			TargetTextMesh.color = newColor;
		}

		/// <summary>
		/// Sets the opacity of the target text
		/// </summary>
		/// <param name="newOpacity"></param>
		public virtual void SetOpacity(float newOpacity)
		{
			_newColor = TargetTextMesh.color;
			_newColor.a = newOpacity;
			TargetTextMesh.color = _newColor;
		}

		/// <summary>
		/// Turns of the text for recycling
		/// </summary>
		protected virtual void TurnOff()
		{
			this.gameObject.SetActive(false);
		}
	}
}