using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to an audio distortion filter to shake its values remapped along a curve 
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Audio/MMAudioFilterDistortionShaker")]
	[RequireComponent(typeof(AudioDistortionFilter))]
	public class MMAudioFilterDistortionShaker : MMShaker
	{
		[MMInspectorGroup("Distortion", true, 51)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeDistortion = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapDistortionZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapDistortionOne = 1f;

		/// the audio source to pilot
		protected AudioDistortionFilter _targetAudioDistortionFilter;
		protected float _initialDistortion;
		protected float _originalShakeDuration;
		protected bool _originalRelativeDistortion;
		protected AnimationCurve _originalShakeDistortion;
		protected float _originalRemapDistortionZero;
		protected float _originalRemapDistortionOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_targetAudioDistortionFilter = this.gameObject.GetComponent<AudioDistortionFilter>();
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
			float newDistortionLevel = ShakeFloat(ShakeDistortion, RemapDistortionZero, RemapDistortionOne, RelativeDistortion, _initialDistortion);
			_targetAudioDistortionFilter.distortionLevel = newDistortionLevel;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialDistortion = _targetAudioDistortionFilter.distortionLevel;
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
		public virtual void OnMMAudioFilterDistortionShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
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
				_originalShakeDistortion = ShakeDistortion;
				_originalRemapDistortionZero = RemapDistortionZero;
				_originalRemapDistortionOne = RemapDistortionOne;
				_originalRelativeDistortion = RelativeDistortion;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeDistortion = distortionCurve;
				RemapDistortionZero = remapMin * feedbacksIntensity;
				RemapDistortionOne = remapMax * feedbacksIntensity;
				RelativeDistortion = relativeDistortion;
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
			_targetAudioDistortionFilter.distortionLevel = _initialDistortion;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeDistortion = _originalShakeDistortion;
			RemapDistortionZero = _originalRemapDistortionZero;
			RemapDistortionOne = _originalRemapDistortionOne;
			RelativeDistortion = _originalRelativeDistortion;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMAudioFilterDistortionShakeEvent.Register(OnMMAudioFilterDistortionShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMAudioFilterDistortionShakeEvent.Unregister(OnMMAudioFilterDistortionShakeEvent);
		}
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMAudioFilterDistortionShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(distortionCurve, duration, remapMin, remapMax, relativeDistortion,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}