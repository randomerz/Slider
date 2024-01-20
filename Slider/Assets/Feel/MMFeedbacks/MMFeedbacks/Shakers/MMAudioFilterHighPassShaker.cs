using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to an audio high pass filter to shake its values remapped along a curve 
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterHighPassShaker")]
	[RequireComponent(typeof(AudioHighPassFilter))]
	public class MMAudioFilterHighPassShaker : MMShaker
	{
		[MMInspectorGroup("High Pass", true, 53)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeHighPass = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeHighPass = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(10f, 22000f)]
		public float RemapHighPassZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(10f, 22000f)]
		public float RemapHighPassOne = 10000f;

		/// the audio source to pilot
		protected AudioHighPassFilter _targetAudioHighPassFilter;
		protected float _initialHighPass;
		protected float _originalShakeDuration;
		protected bool _originalRelativeHighPass;
		protected AnimationCurve _originalShakeHighPass;
		protected float _originalRemapHighPassZero;
		protected float _originalRemapHighPassOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_targetAudioHighPassFilter = this.gameObject.GetComponent<AudioHighPassFilter>();
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newHighPassLevel = ShakeFloat(ShakeHighPass, RemapHighPassZero, RemapHighPassOne, RelativeHighPass, _initialHighPass);
			_targetAudioHighPassFilter.cutoffFrequency = newHighPassLevel;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialHighPass = _targetAudioHighPassFilter.cutoffFrequency;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="highPassCurve"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeHighPass"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMAudioFilterHighPassShakeEvent(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
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
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeHighPass = ShakeHighPass;
				_originalRemapHighPassZero = RemapHighPassZero;
				_originalRemapHighPassOne = RemapHighPassOne;
				_originalRelativeHighPass = RelativeHighPass;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeHighPass = highPassCurve;
				RemapHighPassZero = remapMin * feedbacksIntensity;
				RemapHighPassOne = remapMax * feedbacksIntensity;
				RelativeHighPass = relativeHighPass;
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
			_targetAudioHighPassFilter.cutoffFrequency = _initialHighPass;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeHighPass = _originalShakeHighPass;
			RemapHighPassZero = _originalRemapHighPassZero;
			RemapHighPassOne = _originalRemapHighPassOne;
			RelativeHighPass = _originalRelativeHighPass;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterHighPassShakeEvent.Register(OnMMAudioFilterHighPassShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterHighPassShakeEvent.Unregister(OnMMAudioFilterHighPassShakeEvent);
		}
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMAudioFilterHighPassShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve highPassCurve, float duration, float remapMin, float remapMax, bool relativeHighPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(highPassCurve, duration, remapMin, remapMax, relativeHighPass,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}