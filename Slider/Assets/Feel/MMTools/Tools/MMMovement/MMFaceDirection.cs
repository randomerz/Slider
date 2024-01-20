using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MoreMountains.Tools
{
	/// <summary>
	/// Use this script to have a Transform automatically face a certain direction, whether its own movement direction, or a specific target Transform
	/// </summary>
	public class MMFaceDirection : MonoBehaviour
	{
		/// the possible Updates this script should run at 
		public enum UpdateModes { Update, LateUpdate, FixedUpdate }
		/// the vector to point towards the direction
		public enum ForwardVectors { Forward, Up, Right }
		/// whether to point at this transform's movement direction, or at a target
		public enum FacingModes { MovementDirection, Target }
        
		[Header("Facing Mode")]
		/// whether to point at this transform's movement direction, or at a target
		public FacingModes FacingMode = FacingModes.MovementDirection;
		/// the target to face
		[MMEnumCondition("FacingMode", (int) FacingModes.Target)]
		public Transform FacingTarget;
		/// the minimum distance to consider when computing the movement direction
		[MMEnumCondition("FacingMode", (int) FacingModes.MovementDirection)]
		public float MinimumMovementThreshold = 0.2f;
        
		[Header("Directions")]
		/// the vector to point towards the direction
		public ForwardVectors ForwardVector = ForwardVectors.Forward;
		/// the angles by which to rotate the direction (in degrees)
		public Vector3 DirectionRotationAngles = Vector3.zero;

		[Header("Axis Locks")] 
		public bool LockXAxis = false;
		public bool LockYAxis = false;
		public bool LockZAxis = false;
        
		[Header("Timing")]
		/// the possible Updates this script should run at
		public UpdateModes UpdateMode = UpdateModes.LateUpdate;
		/// the speed at which to interpolate the rotation
		public float InterpolationSpeed = 0.15f;
        
		protected Vector3 _direction;
		protected Vector3 _positionLastFrame;
		protected Transform _transform;
		protected Vector3 _upwards;
		protected Vector3 _targetPosition;

		/// <summary>
		/// On Awake we initialize our behaviour
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Caches upwards vector and transform 
		/// </summary>
		protected virtual void Initialization()
		{
			_transform = this.transform;
			_positionLastFrame = _transform.position;
			switch (ForwardVector)
			{
				case ForwardVectors.Forward:
					_upwards = Vector3.forward;
					break;
				case ForwardVectors.Up:
					_upwards = Vector3.up;
					break;
				case ForwardVectors.Right:
					_upwards = Vector3.right;
					break;
			}
		}

		/// <summary>
		/// Computes the direction to face
		/// </summary>
		protected virtual void FaceDirection()
		{
			if (FacingMode == FacingModes.Target)
			{
				_targetPosition = FacingTarget.position;
				if (LockXAxis) { _targetPosition.x = _transform.position.x; }
				if (LockYAxis) { _targetPosition.y = _transform.position.y; }
				if (LockZAxis) { _targetPosition.z = _transform.position.z; }
	            
				_direction = _targetPosition - _transform.position;
				_direction = Quaternion.Euler(DirectionRotationAngles.x, DirectionRotationAngles.y, DirectionRotationAngles.z) * _direction;
				ApplyRotation();
			}
            
			if (FacingMode == FacingModes.MovementDirection)
			{
				_direction = (_transform.position - _positionLastFrame).normalized;
				if (LockXAxis) { _direction.x = 0; }
				if (LockYAxis) { _direction.y = 0; }
				if (LockZAxis) { _direction.z = 0; }
				_direction = Quaternion.Euler(DirectionRotationAngles.x, DirectionRotationAngles.y, DirectionRotationAngles.z) * _direction;
                
				if (Vector3.Distance(_transform.position, _positionLastFrame) > MinimumMovementThreshold)
				{
					ApplyRotation();
					_positionLastFrame = _transform.position;    
				}
			}
		}

		/// <summary>
		/// Rotates the transform
		/// </summary>
		protected virtual void ApplyRotation()
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction, _upwards), InterpolationSpeed * Time.time);
		}
        
		/// <summary>
		/// On Update we compute our direction if needed 
		/// </summary>
		protected virtual void Update()
		{
			if (UpdateMode == UpdateModes.Update) { FaceDirection(); }
		}
        
		/// <summary>
		/// On LateUpdate we compute our direction if needed 
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate) { FaceDirection(); }
		}
        
		/// <summary>
		/// On FixedUpdate we compute our direction if needed 
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate) { FaceDirection(); }
		}
	}
}