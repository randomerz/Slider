using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	[System.Serializable]
	public class MMRadioSignalOnValueChange : UnityEvent<float> { }

	/// <summary>
	/// A class used to define a signal, meant to be broadcasted by a MMRadioBroadcaster
	/// It'll output a Level value to broadcast, using one time, persistent or driven modes    
	/// Meant to be extended
	/// </summary>
	public abstract class MMRadioSignal : MonoBehaviour
	{
		/// the possible modes a radio signal can operate on
		/// - one time : plays its signal once, goes back to sleep
		/// - outputs a signal constantly while not sleeping
		/// - driven : lets you drive the level value from another script
		public enum SignalModes { OneTime, Persistent, Driven }
		/// whether this signal operates on scaled or unscaled time
		public enum TimeScales { Unscaled, Scaled }
		/// the level, to read from a MMRadioBroadcaster
		public virtual float Level { get { return CurrentLevel; } }
		/// the time, unscaled or scaled
		public float TimescaleTime { get { return (TimeScale == TimeScales.Scaled) ? Time.time : Time.unscaledTime; } }
		/// the delta time, unscaled or not
		public float TimescaleDeltaTime { get { return (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; } }

		[Header("Signal")]
		/// the selected signal mode
		public SignalModes SignalMode = SignalModes.Persistent;
		/// the selected time scale
		public TimeScales TimeScale = TimeScales.Unscaled;
		/// the duration of the shake, in seconds
		public float Duration = 2f;
		/// a global multiplier to apply to the end result of the combination
		public float GlobalMultiplier = 1f;
		/// the current level, not to be read from a broadcaster (it's best to use the property than the field, fields generate garbage)
		[MMReadOnly]
		public float CurrentLevel = 0f;

		[Header("Play Settings")]
		/// whether or not this shaker is shaking right now
		[MMReadOnly]
		public bool Playing = false;
		/// the driver time, that can be controlled from another class if you're in Driven mode
		[Range(0f, 1f)]
		public float DriverTime;
		/// if this is true this shaker will play on awake
		public bool PlayOnStart = true;
		/// an event to trigger on value change
		public MMRadioSignalOnValueChange OnValueChange;
        
		/// a test button to start shaking 
		[Header(("Debug"))]
		[MMInspectorButton("StartShaking")] 
		public bool StartShakingButton;

		protected float _signalTime = 0f;
		protected float _shakeStartedTimestamp;
		protected float _levelLastFrame;

		/// <summary>
		/// On Awake we grab our volume and profile
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			if (PlayOnStart)
			{
				StartShaking();
			}
			this.enabled = PlayOnStart;
		}

		/// <summary>
		/// Override this method to initialize your shaker
		/// </summary>
		protected virtual void Initialization()
		{
			CurrentLevel = 0f;
		}

		/// <summary>
		/// Starts shaking the values
		/// </summary>
		public virtual void StartShaking()
		{
			if (Playing)
			{
				return;
			}
			else
			{
				this.enabled = true;
				_shakeStartedTimestamp = TimescaleTime;
				Playing = true;
				ShakeStarts();
			}
		}

		/// <summary>
		/// Describes what happens when a shake starts
		/// </summary>
		protected virtual void ShakeStarts()
		{

		}

		/// <summary>
		/// On Update, we shake our values if needed, or reset if our shake has ended
		/// </summary>
		protected virtual void Update()
		{
			ProcessUpdate();

			if (SignalMode == SignalModes.Driven)
			{
				ProcessDrivenMode();
			}
			else if (SignalMode == SignalModes.Persistent)
			{
				_signalTime += TimescaleDeltaTime;
				if (_signalTime > Duration)
				{
					_signalTime = 0f;
				}

				DriverTime = MMMaths.Remap(_signalTime, 0f, Duration, 0f, 1f);
			}
			else if (SignalMode == SignalModes.OneTime)
			{

			}

			if (Playing || (SignalMode == SignalModes.Driven))
			{
				Shake();
			}
                        
			if ((SignalMode == SignalModes.OneTime) && Playing && (TimescaleTime - _shakeStartedTimestamp > Duration))
			{
				ShakeComplete();
			}

			if (_levelLastFrame != Level)
			{
				ApplyLevel(Level);
			}

			_levelLastFrame = Level;
		}

		public virtual void ApplyLevel(float level)
		{
			if (OnValueChange != null)
			{
				OnValueChange.Invoke(level);	
			}
		}

		/// <summary>
		/// A method to override to describe the behaviour in Driven mode
		/// </summary>
		protected virtual void ProcessDrivenMode()
		{

		}

		/// <summary>
		/// A method to override to describe what should happen at update
		/// </summary>
		protected virtual void ProcessUpdate()
		{

		}

		/// <summary>
		/// Override this method to implement shake over time
		/// </summary>
		protected virtual void Shake()
		{

		}

		public virtual float GraphValue(float time)
		{
			return 0f;
		}

		/// <summary>
		/// Describes what happens when the shake is complete
		/// </summary>
		protected virtual void ShakeComplete()
		{
			Playing = false;
			this.enabled = false;
		}

		/// <summary>
		/// On enable we start shaking if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			StartShaking();
		}

		/// <summary>
		/// On destroy we stop listening for events
		/// </summary>
		protected virtual void OnDestroy()
		{

		}

		/// <summary>
		/// On disable we complete our shake if it was in progress
		/// </summary>
		protected virtual void OnDisable()
		{
			if (Playing)
			{
				ShakeComplete();
			}
		}

		/// <summary>
		/// Starts this shaker
		/// </summary>
		public virtual void Play()
		{
			this.enabled = true;
		}

		/// <summary>
		/// Starts this shaker
		/// </summary>
		public virtual void Stop()
		{
			ShakeComplete();
		}
        
		/// <summary>
		/// Applies a bias to a time value
		/// </summary>
		/// <param name="t"></param>
		/// <param name="bias"></param>
		/// <returns></returns>
		public virtual float ApplyBias(float t, float bias)
		{
			if (bias == 0.5f)
			{
				return t;
			}
            
			bias = MMMaths.Remap(bias, 0f, 1f, 1f, 0f);
            
			float a = bias * 2.0f - 1.0f;

			if (a < 0)
			{
				t = 1 - Mathf.Pow(1.0f - t, Mathf.Max(1 + a, .01f));
			}
			else
			{
				t = Mathf.Pow(t, Mathf.Max(1 - a, .01f));
			}

			return t;
		}
	}
}