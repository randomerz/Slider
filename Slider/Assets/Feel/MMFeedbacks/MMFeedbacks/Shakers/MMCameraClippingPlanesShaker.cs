using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to a camera and it'll let you control its near and far clipping planes
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MMCameraClippingPlanesShaker")]
	[RequireComponent(typeof(Camera))]
	public class MMCameraClippingPlanesShaker : MMShaker
	{
		[MMInspectorGroup("Clipping Planes", true, 31)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeClippingPlanes = false;
        
		[MMInspectorGroup("Near Plane", true, 32)]
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeNear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapNearZero = 0.3f;
		/// the value to remap the curve's 1 to        
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapNearOne = 100f;

		[MMInspectorGroup("Far Plane", true, 33)]
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeFar = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapFarZero = 1000f;
		/// the value to remap the curve's 1 to        
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapFarOne = 1000f;
        
		protected Camera _targetCamera;
		protected float _initialNear;
		protected float _initialFar;

		protected float _originalShakeDuration;
		protected bool _originalRelativeClippingPlanes;

		protected AnimationCurve _originalShakeNear;
		protected float _originalRemapNearZero;
		protected float _originalRemapNearOne;

		protected AnimationCurve _originalShakeFar;
		protected float _originalRemapFarZero;
		protected float _originalRemapFarOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_targetCamera = this.gameObject.GetComponent<Camera>();
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newNear = ShakeFloat(ShakeNear, RemapNearZero, RemapNearOne, RelativeClippingPlanes, _initialNear);
			_targetCamera.nearClipPlane = newNear;
			float newFar = ShakeFloat(ShakeFar, RemapFarZero, RemapFarOne, RelativeClippingPlanes, _initialFar);
			_targetCamera.farClipPlane = newFar;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialNear = _targetCamera.nearClipPlane;
			_initialFar = _targetCamera.farClipPlane;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="distortionCurve"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeDistortion"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMCameraClippingPlanesShakeEvent(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValues = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData))
			{
				return;
			}
            
			if (stop)
			{
				Stop();
				return;
			}
            
			if (restore)
			{
				ResetTargetValues();
				return;
			}
            
			if (!Interruptible && Shaking)
			{
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeNear = ShakeNear;
				_originalShakeFar = ShakeFar;
				_originalRemapNearZero = RemapNearZero;
				_originalRemapNearOne = RemapNearOne;
				_originalRemapFarZero = RemapFarZero;
				_originalRemapFarOne = RemapFarOne;
				_originalRelativeClippingPlanes = RelativeClippingPlanes;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeNear = animNearCurve;
				RemapNearZero = remapNearMin * feedbacksIntensity;
				RemapNearOne = remapNearMax * feedbacksIntensity;
				ShakeFar = animFarCurve;
				RemapFarZero = remapFarMin * feedbacksIntensity;
				RemapFarOne = remapFarMax * feedbacksIntensity;
				RelativeClippingPlanes = relativeValues;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_targetCamera.nearClipPlane = _initialNear;
			_targetCamera.farClipPlane = _initialFar;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeNear = _originalShakeNear;
			ShakeFar = _originalShakeFar;
			RemapNearZero = _originalRemapNearZero;
			RemapNearOne = _originalRemapNearOne;
			RemapFarZero = _originalRemapFarZero;
			RemapFarOne = _originalRemapFarOne;
			RelativeClippingPlanes = _originalRelativeClippingPlanes;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMCameraClippingPlanesShakeEvent.Register(OnMMCameraClippingPlanesShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMCameraClippingPlanesShakeEvent.Unregister(OnMMCameraClippingPlanesShakeEvent);
		}
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMCameraClippingPlanesShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve animNearCurve, float duration, float remapNearMin, float remapNearMax, AnimationCurve animFarCurve, float remapFarMin, float remapFarMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(animNearCurve, duration, remapNearMin, remapNearMax, animFarCurve, remapFarMin, remapFarMax, relativeValue,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}