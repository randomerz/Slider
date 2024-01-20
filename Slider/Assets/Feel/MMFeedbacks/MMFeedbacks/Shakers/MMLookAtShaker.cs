using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to an object and it'll be able to listen for MMFLookAtShakeEvents, and when one is received, it will rotate its associated transform accordingly
	/// </summary>
	public class MMLookAtShaker : MMShaker
	{
		[MMInspectorGroup("Look at settings", true, 37)]
		/// the duration of this shake, in seconds
		[Tooltip("the duration of this shake, in seconds")]
		public float Duration = 1f;
		/// the curve over which to animate the look at transition
		[Tooltip("the curve over which to animate the look at transition")]
		public MMTweenType LookAtTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// whether or not to lock rotation on the x axis
		[Tooltip("whether or not to lock rotation on the x axis")]
		public bool LockXAxis = false;
		/// whether or not to lock rotation on the y axis
		[Tooltip("whether or not to lock rotation on the y axis")]
		public bool LockYAxis = false;
		/// whether or not to lock rotation on the z axis
		[Tooltip("whether or not to lock rotation on the z axis")]
		public bool LockZAxis = false;

		[MMInspectorGroup("What we want to rotate", true, 37)]
		/// in Direct mode, the transform to rotate to have it look at our target - if left empty, will be the transform this shaker is on
		[Tooltip("in Direct mode, the transform to rotate to have it look at our target - if left empty, will be the transform this shaker is on")]
		public Transform TransformToRotate;
		/// the vector representing the up direction on the object we want to rotate and look at our target
		public MMF_LookAt.UpwardVectors UpwardVector = MMF_LookAt.UpwardVectors.Up;

		[MMInspectorGroup("What we want to look at", true, 37)]
		/// the different target modes : either a specific transform to look at, the coordinates of a world position, or a direction vector
		[Tooltip("the different target modes : either a specific transform to look at, the coordinates of a world position, or a direction vector")]
		public MMF_LookAt.LookAtTargetModes LookAtTargetMode = MMF_LookAt.LookAtTargetModes.Transform;
		/// the transform we want to look at 
		[Tooltip("the transform we want to look at")]
		[MMFEnumCondition("LookAtTargetMode", (int)MMF_LookAt.LookAtTargetModes.Transform)]
		public Transform LookAtTarget;
		/// the coordinates of a point the world that we want to look at
		[Tooltip("the coordinates of a point the world that we want to look at")]
		[MMFEnumCondition("LookAtTargetMode", (int)MMF_LookAt.LookAtTargetModes.TargetWorldPosition)]
		public Vector3 LookAtTargetWorldPosition = Vector3.forward;
		/// a direction (from our rotating object) that we want to look at
		[Tooltip("a direction (from our rotating object) that we want to look at")]
		[MMFEnumCondition("LookAtTargetMode", (int)MMF_LookAt.LookAtTargetModes.Direction)]
		public Vector3 LookAtDirection = Vector3.forward;
		
		[MMInspectorGroup("Test", true, 46)]
		[MMInspectorButton("StartShaking")] 
		public bool StartShakingButton;
		
		/// <summary>
		/// An event used to trigger a look at shake 
		/// </summary>
		public struct MMLookAtShakeEvent
		{
			static private event Delegate OnEvent;
			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
			static public void Register(Delegate callback) { OnEvent += callback; }
			static public void Unregister(Delegate callback) { OnEvent -= callback; }

			public delegate void Delegate(float duration, 
				bool lockXAxis, bool lockYAxis, bool lockZAxis, MMF_LookAt.UpwardVectors upwardVector, MMF_LookAt.LookAtTargetModes lookAtTargetMode,Transform lookAtTarget, Vector3 lookAtTargetWorldPosition, Vector3 lookAtDirection, Transform transformToRotate, MMTweenType lookAtTween,
				bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
				float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
				bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false);

			static public void Trigger(float duration, 
				bool lockXAxis, bool lockYAxis, bool lockZAxis, MMF_LookAt.UpwardVectors upwardVector, MMF_LookAt.LookAtTargetModes lookAtTargetMode,Transform lookAtTarget, Vector3 lookAtTargetWorldPosition, Vector3 lookAtDirection, Transform transformToRotate, MMTweenType lookAtTween,
				bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
				float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
				bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
			{
				OnEvent?.Invoke( duration, lockXAxis, lockYAxis, lockZAxis, upwardVector, lookAtTargetMode, lookAtTarget, lookAtTargetWorldPosition, lookAtDirection, transformToRotate, lookAtTween,
					useRange, rangeDistance, useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition,
					feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
			}
		}
		
		protected Quaternion _newRotation;
		protected Vector3 _lookAtPosition;
		protected Vector3 _upwards;
		protected Vector3 _direction;
		protected Quaternion _initialRotation;
		protected float _originalDuration = 1f;
		protected MMTweenType _originalLookAtTween;
		protected bool _originalLockXAxis;
		protected bool _originalLockYAxis;
		protected bool _originalLockZAxis;
		protected MMF_LookAt.UpwardVectors _originalUpwardVector;
		protected MMF_LookAt.LookAtTargetModes _originalLookAtTargetMode;
		protected Transform _originalLookAtTarget;
		protected Vector3 _originalLookAtTargetWorldPosition;
		protected Vector3 _originalLookAtDirection;
		
		/// <summary>
		/// On init we store our initial rotation
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (TransformToRotate == null)
			{
				TransformToRotate = this.transform;
			}
			_initialRotation = TransformToRotate.rotation;
		}
		
		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}
        
		/// <summary>
		/// On shake we apply rotation on our target transform
		/// </summary>
		protected override void Shake()
		{
			ApplyRotation(_journey);
		}
        
		/// <summary>
		/// On shake complete, we apply our final rotation
		/// </summary>
		protected override void ShakeComplete()
		{
			ApplyRotation(1f);
			base.ShakeComplete();
		}
		
		/// <summary>
		/// Rotates the associated transform to look at our target
		/// </summary>
		/// <param name="journey"></param>
		protected virtual void ApplyRotation(float journey)
		{
			float percent = Mathf.Clamp01(journey / ShakeDuration);
			percent = LookAtTween.Evaluate(percent);
			
			switch (LookAtTargetMode)
			{
				case MMF_LookAt.LookAtTargetModes.Transform:
					_lookAtPosition = LookAtTarget.position;
					break;
				case MMF_LookAt.LookAtTargetModes.TargetWorldPosition:
					_lookAtPosition = LookAtTargetWorldPosition;
					break;
				case MMF_LookAt.LookAtTargetModes.Direction:
					_lookAtPosition = TransformToRotate.position + LookAtDirection;
					break;
			}
			
			if (LockXAxis) { _lookAtPosition.x = TransformToRotate.position.x; }
			if (LockYAxis) { _lookAtPosition.y = TransformToRotate.position.y; }
			if (LockZAxis) { _lookAtPosition.z = TransformToRotate.position.z; }
	            
			_direction = _lookAtPosition - TransformToRotate.position;
			_newRotation = Quaternion.LookRotation(_direction, _upwards);
			
			TransformToRotate.transform.rotation = Quaternion.SlerpUnclamped(_initialRotation, _newRotation, percent);
		}
		
		/// <summary>
		/// When getting a new look at event, we make our transform look at the specified target
		/// </summary>
		public virtual void OnMMLookAtShakeEvent(float duration, 
			bool lockXAxis, bool lockYAxis, bool lockZAxis, MMF_LookAt.UpwardVectors upwardVector, MMF_LookAt.LookAtTargetModes lookAtTargetMode,Transform lookAtTarget, Vector3 lookAtTargetWorldPosition, Vector3 lookAtDirection, Transform transformToRotate, MMTweenType lookAtTween,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
		{
			if (!CheckEventAllowed(channelData, useRange, rangeDistance, rangePosition) || (!Interruptible && Shaking))
			{
				return;
			}
            
			if (stop)
			{
				Stop();
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalDuration = ShakeDuration;
				_originalLookAtTween = LookAtTween;
				_originalLockXAxis = LockXAxis;
				_originalLockYAxis = LockYAxis;
				_originalLockZAxis = LockZAxis;
				_originalUpwardVector = UpwardVector;
				_originalLookAtTargetMode = LookAtTargetMode;
				_originalLookAtTarget = LookAtTarget;
				_originalLookAtTargetWorldPosition = LookAtTargetWorldPosition;
				_originalLookAtDirection = LookAtDirection;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				LookAtTween = lookAtTween;
				LockXAxis = lockXAxis;
				LockYAxis = lockYAxis;
				LockZAxis = lockZAxis;
				UpwardVector = upwardVector;
				LookAtTargetMode = lookAtTargetMode;
				LookAtTarget = lookAtTarget;
				LookAtTargetWorldPosition = lookAtTargetWorldPosition;
				LookAtDirection = lookAtDirection;
				ForwardDirection = forwardDirection;
			}

			Play();
		}
		
		/// <summary>
		/// On ResetTargetValue, we reset our target transform's rotation
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			TransformToRotate.rotation = _initialRotation;
		}
		
		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalDuration;
			LookAtTween = _originalLookAtTween;
			LockXAxis = _originalLockXAxis;
			LockYAxis = _originalLockYAxis;
			LockZAxis = _originalLockZAxis;
			UpwardVector = _originalUpwardVector;
			LookAtTargetMode = _originalLookAtTargetMode;
			LookAtTarget = _originalLookAtTarget;
			LookAtTargetWorldPosition = _originalLookAtTargetWorldPosition;
			LookAtDirection = _originalLookAtDirection;
		}
		
		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMLookAtShakeEvent.Register(OnMMLookAtShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMLookAtShakeEvent.Unregister(OnMMLookAtShakeEvent);
		}
	}
}