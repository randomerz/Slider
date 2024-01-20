using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to an audio distortion low pass to shake its values remapped along a curve 
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterLowPassShaker")]
	[RequireComponent(typeof(AudioLowPassFilter))]
	public class MMAudioFilterLowPassShaker : MMShaker
	{
		[MMInspectorGroup("Low Pass", true, 54)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeLowPass = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeLowPass = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(10f, 22000f)]
		public float RemapLowPassZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(10f, 22000f)]
		public float RemapLowPassOne = 10000f;

		/// the audio source to pilot
		protected AudioLowPassFilter _targetAudioLowPassFilter;
		protected float _initialLowPass;
		protected float _originalShakeDuration;
		protected bool _originalRelativeLowPass;
		protected AnimationCurve _originalShakeLowPass;
		protected float _originalRemapLowPassZero;
		protected float _originalRemapLowPassOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_targetAudioLowPassFilter = this.gameObject.GetComponent<AudioLowPassFilter>();
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
			float newLowPassLevel = ShakeFloat(ShakeLowPass, RemapLowPassZero, RemapLowPassOne, RelativeLowPass, _initialLowPass);
			_targetAudioLowPassFilter.cutoffFrequency = newLowPassLevel;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialLowPass = _targetAudioLowPassFilter.cutoffFrequency;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="lowPassCurve"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeLowPass"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMAudioFilterLowPassShakeEvent(AnimationCurve lowPassCurve, float duration, float remapMin, float remapMax, bool relativeLowPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
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
				_originalShakeLowPass = ShakeLowPass;
				_originalRemapLowPassZero = RemapLowPassZero;
				_originalRemapLowPassOne = RemapLowPassOne;
				_originalRelativeLowPass = RelativeLowPass;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeLowPass = lowPassCurve;
				RemapLowPassZero = remapMin * feedbacksIntensity;
				RemapLowPassOne = remapMax * feedbacksIntensity;
				RelativeLowPass = relativeLowPass;
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
			_targetAudioLowPassFilter.cutoffFrequency = _initialLowPass;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeLowPass = _originalShakeLowPass;
			RemapLowPassZero = _originalRemapLowPassZero;
			RemapLowPassOne = _originalRemapLowPassOne;
			RelativeLowPass = _originalRelativeLowPass;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterLowPassShakeEvent.Register(OnMMAudioFilterLowPassShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterLowPassShakeEvent.Unregister(OnMMAudioFilterLowPassShakeEvent);
		}
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMAudioFilterLowPassShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve lowPassCurve, float duration, float remapMin, float remapMax, bool relativeLowPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve lowPassCurve, float duration, float remapMin, float remapMax, bool relativeLowPass = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(lowPassCurve, duration, remapMin, remapMax, relativeLowPass,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}