using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to a camera and it'll let you control its field of view over time, can be piloted by a MMFeedbackCameraFieldOfView
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MMCameraFieldOfViewShaker")]
	[RequireComponent(typeof(Camera))]
	public class MMCameraFieldOfViewShaker : MMShaker
	{
		[MMInspectorGroup("Field of View", true, 34)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeFieldOfView = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeFieldOfView = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 179f)]
		public float RemapFieldOfViewZero = 60f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 179f)]
		public float RemapFieldOfViewOne = 120f;

		protected Camera _targetCamera;
		protected float _initialFieldOfView;
		protected float _originalShakeDuration;
		protected bool _originalRelativeFieldOfView;
		protected AnimationCurve _originalShakeFieldOfView;
		protected float _originalRemapFieldOfViewZero;
		protected float _originalRemapFieldOfViewOne;

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
			float newFieldOfView = ShakeFloat(ShakeFieldOfView, RemapFieldOfViewZero, RemapFieldOfViewOne, RelativeFieldOfView, _initialFieldOfView);
			_targetCamera.fieldOfView = newFieldOfView;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialFieldOfView = _targetCamera.fieldOfView;
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
		public virtual void OnMMCameraFieldOfViewShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
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
				_originalShakeFieldOfView = ShakeFieldOfView;
				_originalRemapFieldOfViewZero = RemapFieldOfViewZero;
				_originalRemapFieldOfViewOne = RemapFieldOfViewOne;
				_originalRelativeFieldOfView = RelativeFieldOfView;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeFieldOfView = distortionCurve;
				RemapFieldOfViewZero = remapMin * feedbacksIntensity;
				RemapFieldOfViewOne = remapMax * feedbacksIntensity;
				RelativeFieldOfView = relativeDistortion;
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
			_targetCamera.fieldOfView = _initialFieldOfView;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeFieldOfView = _originalShakeFieldOfView;
			RemapFieldOfViewZero = _originalRemapFieldOfViewZero;
			RemapFieldOfViewOne = _originalRemapFieldOfViewOne;
			RelativeFieldOfView = _originalRelativeFieldOfView;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMCameraFieldOfViewShakeEvent.Register(OnMMCameraFieldOfViewShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMCameraFieldOfViewShakeEvent.Unregister(OnMMCameraFieldOfViewShakeEvent);
		}
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMCameraFieldOfViewShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve animCurve, float duration, float remapMin, float remapMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve animCurve, float duration, float remapMin, float remapMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(animCurve, duration, remapMin, remapMax, relativeValue,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}