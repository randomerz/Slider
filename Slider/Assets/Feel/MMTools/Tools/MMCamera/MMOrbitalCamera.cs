using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to make a camera orbit around a target
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Camera/MMOrbitalCamera")]
	public class MMOrbitalCamera : MonoBehaviour
	{
		/// the possible input modes for this camera
		public enum Modes { Mouse, Touch }

		[Header("Setup")]
		/// the selected input mode
		public Modes Mode = Modes.Touch;
		/// the object to orbit around
		public Transform Target;
		/// the offset to apply while orbiting
		public Vector3 TargetOffset;
		/// the current distance to target
		[MMReadOnly]        
		public float DistanceToTarget = 5f;

		[Header("Rotation")]
		/// whether or not rotation is enabled
		public bool RotationEnabled = true;
		/// the speed of the rotation
		public Vector2 RotationSpeed = new Vector2(200f, 200f);
		/// the minimum vertical angle limit
		public int MinVerticalAngleLimit = -80;
		/// the maximum vertical angle limit
		public int MaxVerticalAngleLimit = 80;

		[Header("Zoom")]
		/// whether or not zoom is enabled
		public bool ZoomEnabled = true;
		/// the minimum distance at which the user can zoom in
		public float MinimumZoomDistance = 0.6f;
		/// the max distance at which the user can zoom out
		public float MaximumZoomDistance = 20;
		/// the speed of the zoom interpolation
		public int ZoomSpeed = 40;
		/// the dampening to apply to the zoom
		public float ZoomDampening = 5f;

		[Header("Mouse Zoom")]
		/// the speed at which scrolling the mouse wheel will zoom
		public float MouseWheelSpeed = 10f;
		/// the max value at which to clamp the mouse wheel
		public float MaxMouseWheelClamp = 10f;

		[Header("Steps")]
		/// the distance after which to trigger a step
		public float StepThreshold = 1;
		/// an event to trigger when a step is met
		public UnityEvent StepFeedback;

		protected float _angleX = 0f;
		protected float _angleY = 0f;
		protected float _currentDistance;
		protected float _desiredDistance;
		protected Quaternion _currentRotation;
		protected Quaternion _desiredRotation;
		protected Quaternion _rotation;
		protected Vector3 _position;
		protected float _scrollWheelAmount = 0;
		protected float _stepBuffer = 0f;

		/// <summary>
		/// On Start we initialize our orbital camera
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we store our positions and rotations
		/// </summary>
		public virtual void Initialization()
		{
			// if no target is set, we throw an error and exit
			if (Target == null)
			{
				Debug.LogError(this.gameObject.name + " : the MMOrbitalCamera doesn't have a target.");
				return;
			}

			DistanceToTarget = Vector3.Distance(Target.position, transform.position);
			_currentDistance = DistanceToTarget;
			_desiredDistance = DistanceToTarget;

			_position = transform.position;
			_rotation = transform.rotation;
			_currentRotation = transform.rotation;
			_desiredRotation = transform.rotation;

			_angleX = Vector3.Angle(Vector3.right, transform.right);
			_angleY = Vector3.Angle(Vector3.up, transform.up);
		}

		/// <summary>
		/// On late update we rotate, zoom, detect steps and finally apply our movement
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (Target == null)
			{
				return;
			}

			Rotation();
			Zoom();
			StepDetection();
			ApplyMovement();
		}

		/// <summary>
		/// Rotates the camera around the object
		/// </summary>
		protected virtual void Rotation()
		{
			if (!RotationEnabled)
			{
				return;
			}

			if (Mode == Modes.Touch && (Input.touchCount > 0))
			{
				if ((Input.touches[0].phase == TouchPhase.Moved) && (Input.touchCount == 1))
				{
					float screenHeight = Screen.currentResolution.height;
					if (Input.touches[0].position.y < screenHeight/4)
					{
						return;
					}

					float swipeSpeed = Input.touches[0].deltaPosition.magnitude / Input.touches[0].deltaTime;

					_angleX += Input.touches[0].deltaPosition.x * RotationSpeed.x * Time.deltaTime * swipeSpeed * 0.00001f;
					_angleY -= Input.touches[0].deltaPosition.y * RotationSpeed.y * Time.deltaTime * swipeSpeed * 0.00001f;
					_stepBuffer += Input.touches[0].deltaPosition.x;

					_angleY = MMMaths.ClampAngle(_angleY, MinVerticalAngleLimit, MaxVerticalAngleLimit);
					_desiredRotation = Quaternion.Euler(_angleY, _angleX, 0);
					_currentRotation = transform.rotation;

					_rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * ZoomDampening);
					transform.rotation = _rotation;
				}
				else if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
				{
					_desiredRotation = transform.rotation;
				}

				if (transform.rotation != _desiredRotation)
				{
					_rotation = Quaternion.Lerp(transform.rotation, _desiredRotation, Time.deltaTime * ZoomDampening);
					transform.rotation = _rotation;
				}
			}
			else if (Mode == Modes.Mouse)
			{
				_angleX += Input.GetAxis("Mouse X") * RotationSpeed.x * Time.deltaTime;
				_angleY += -Input.GetAxis("Mouse Y") * RotationSpeed.y * Time.deltaTime;
				_angleY = Mathf.Clamp(_angleY, MinVerticalAngleLimit, MaxVerticalAngleLimit);

				_desiredRotation = Quaternion.Euler(new Vector3(_angleY, _angleX, 0));
				_currentRotation = transform.rotation;
				_rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * ZoomDampening);
				transform.rotation = _rotation;
			}            
		}

		/// <summary>
		/// Detects steps 
		/// </summary>
		protected virtual void StepDetection()
		{
			if (Mathf.Abs(_stepBuffer) > StepThreshold)
			{
				StepFeedback?.Invoke();
				_stepBuffer = 0f;
			}
		}

		/// <summary>
		/// Zooms
		/// </summary>
		protected virtual void Zoom()
		{
			if (!ZoomEnabled)
			{
				return;
			}

			if (Mode == Modes.Touch && (Input.touchCount > 0))
			{
				if (Input.touchCount == 2)
				{
					Touch firstTouch = Input.GetTouch(0);
					Touch secondTouch = Input.GetTouch(1);

					Vector2 firstTouchPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
					Vector2 secondTouchPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

					float previousTouchDeltaMagnitude = (firstTouchPreviousPosition - secondTouchPreviousPosition).magnitude;
					float thisTouchDeltaMagnitude = (firstTouch.position - secondTouch.position).magnitude;
					float deltaMagnitudeDifference = previousTouchDeltaMagnitude - thisTouchDeltaMagnitude;

					_desiredDistance += deltaMagnitudeDifference * Time.deltaTime * ZoomSpeed * Mathf.Abs(_desiredDistance) * 0.001f;
					_desiredDistance = Mathf.Clamp(_desiredDistance, MinimumZoomDistance, MaximumZoomDistance);
					_currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * ZoomDampening);
				}
			}
			else if (Mode == Modes.Mouse)
			{
				_scrollWheelAmount += - Input.GetAxis("Mouse ScrollWheel") * MouseWheelSpeed;
				_scrollWheelAmount = Mathf.Clamp(_scrollWheelAmount, -MaxMouseWheelClamp, MaxMouseWheelClamp);
                
				float deltaMagnitudeDifference = _scrollWheelAmount;

				_desiredDistance += deltaMagnitudeDifference * Time.deltaTime * ZoomSpeed * Mathf.Abs(_desiredDistance) * 0.001f;
				_desiredDistance = Mathf.Clamp(_desiredDistance, MinimumZoomDistance, MaximumZoomDistance);
				_currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * ZoomDampening);

			}
		}

		/// <summary>
		/// Moves the transform
		/// </summary>
		protected virtual void ApplyMovement()
		{
			_position = Target.position - (_rotation * Vector3.forward * _currentDistance + TargetOffset);
			transform.position = _position;
		}
	}
}