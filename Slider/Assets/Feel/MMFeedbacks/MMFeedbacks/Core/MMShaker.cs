using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	public class MMShaker : MMMonoBehaviour
	{
		[MMInspectorGroup("Shaker Settings", true, 3)]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
		         "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("the channel to listen to - has to match the one on the feedback")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
		         "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float ShakeDuration = 0.2f;
		/// if this is true this shaker will play on awake
		[Tooltip("if this is true this shaker will play on awake")]
		public bool PlayOnAwake = false;
		/// if this is true, the shaker will shake permanently as long as its game object is active
		[Tooltip("if this is true, the shaker will shake permanently as long as its game object is active")]
		public bool PermanentShake = false;
		/// if this is true, a new shake can happen while shaking
		[Tooltip("if this is true, a new shake can happen while shaking")]
		public bool Interruptible = true;
		/// if this is true, this shaker will always reset target values, regardless of how it was called
		[Tooltip("if this is true, this shaker will always reset target values, regardless of how it was called")]
		public bool AlwaysResetTargetValuesAfterShake = false;
		/// if this is true, this shaker will ignore any value passed in an event that triggered it, and will instead use the values set on its inspector
		[Tooltip("if this is true, this shaker will ignore any value passed in an event that triggered it, and will instead use the values set on its inspector")]
		public bool OnlyUseShakerValues = false;
		/// a cooldown, in seconds, after a shake, during which no other shake can start
		[Tooltip("a cooldown, in seconds, after a shake, during which no other shake can start")]
		public float CooldownBetweenShakes = 0f;
		/// whether or not this shaker is shaking right now
		[Tooltip("whether or not this shaker is shaking right now")]
		[MMFReadOnly]
		public bool Shaking = false;
        
		[HideInInspector] 
		public bool ForwardDirection = true;

		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		public virtual MMChannelData ChannelData => new MMChannelData(ChannelMode, Channel, MMChannelDefinition);
        
		public bool ListeningToEvents => _listeningToEvents;

		[HideInInspector]
		internal bool _listeningToEvents = false;
		protected float _shakeStartedTimestamp = -Single.MaxValue;
		protected float _remappedTimeSinceStart;
		protected bool _resetShakerValuesAfterShake;
		protected bool _resetTargetValuesAfterShake;
		protected float _journey;
        
		/// <summary>
		/// On Awake we grab our volume and profile
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			// in case someone else trigger StartListening before Awake
			if (!_listeningToEvents)
			{
				StartListening();
			}
			Shaking = PlayOnAwake;
			this.enabled = PlayOnAwake;
		}

		/// <summary>
		/// Override this method to initialize your shaker
		/// </summary>
		protected virtual void Initialization()
		{
		}

		/// <summary>
		/// Call this externally if you need to force a new initialization
		/// </summary>
		public virtual void ForceInitialization()
		{
			Initialization();
		}

		/// <summary>
		/// Starts shaking the values
		/// </summary>
		public virtual void StartShaking()
		{
			_journey = ForwardDirection ? 0f : ShakeDuration;

			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
            
			if (Shaking)
			{
				return;
			}
			else
			{
				this.enabled = true;
				_shakeStartedTimestamp = GetTime();
				Shaking = true;
				GrabInitialValues();
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
		/// A method designed to collect initial values
		/// </summary>
		protected virtual void GrabInitialValues()
		{

		}

		/// <summary>
		/// On Update, we shake our values if needed, or reset if our shake has ended
		/// </summary>
		protected virtual void Update()
		{
			if (Shaking || PermanentShake)
			{
				Shake();
				_journey += ForwardDirection ? GetDeltaTime() : -GetDeltaTime();
			}

			if (Shaking && !PermanentShake && ((_journey < 0) || (_journey > ShakeDuration)))
			{
				Shaking = false;
				ShakeComplete();
			}

			if (PermanentShake)
			{
				if (_journey < 0)
				{
					_journey = ShakeDuration;
				}

				if (_journey > ShakeDuration)
				{
					_journey = 0;
				}
			}
		}

		/// <summary>
		/// Override this method to implement shake over time
		/// </summary>
		protected virtual void Shake()
		{

		}

		/// <summary>
		/// A method used to "shake" a flot over time along a curve
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="remapMin"></param>
		/// <param name="remapMax"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="initialValue"></param>
		/// <returns></returns>
		protected virtual float ShakeFloat(AnimationCurve curve, float remapMin, float remapMax, bool relativeIntensity, float initialValue)
		{
			float newValue = 0f;
            
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
            
			float curveValue = curve.Evaluate(remappedTime);
			newValue = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, remapMin, remapMax);
			if (relativeIntensity)
			{
				newValue += initialValue;
			}
			return newValue;
		}

		/// <summary>
		/// Resets the values on the target
		/// </summary>
		protected virtual void ResetTargetValues()
		{

		}

		/// <summary>
		/// Resets the values on the shaker
		/// </summary>
		protected virtual void ResetShakerValues()
		{

		}

		/// <summary>
		/// Describes what happens when the shake is complete
		/// </summary>
		protected virtual void ShakeComplete()
		{
			_journey = ForwardDirection ? ShakeDuration : 0f;
			Shake();
			
			if (_resetTargetValuesAfterShake || AlwaysResetTargetValuesAfterShake)
			{
				ResetTargetValues();
			}   
			if (_resetShakerValuesAfterShake)
			{
				ResetShakerValues();
			}            
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
			StopListening();
		}

		/// <summary>
		/// On disable we complete our shake if it was in progress
		/// </summary>
		protected virtual void OnDisable()
		{
			if (Shaking)
			{
				ShakeComplete();
			}
		}

		/// <summary>
		/// Starts this shaker
		/// </summary>
		public virtual void Play()
		{
			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
			this.enabled = true;
		}

		/// <summary>
		/// Stops this shaker
		/// </summary>
		public virtual void Stop()
		{
			Shaking = false;
			ShakeComplete();
		}
        
		/// <summary>
		/// Starts listening for events
		/// </summary>
		public virtual void StartListening()
		{
			_listeningToEvents = true;
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void StopListening()
		{
			_listeningToEvents = false;
		}

		/// <summary>
		/// Returns true if this shaker should listen to events, false otherwise
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		protected virtual bool CheckEventAllowed(MMChannelData channelData, bool useRange = false, float range = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return false;
			}
			if (!this.gameObject.activeInHierarchy)
			{
				return false;
			}
			else
			{
				if (useRange)
				{
					if (Vector3.Distance(this.transform.position, eventOriginPosition) > range)
					{
						return false;
					}
				}

				return true;
			}
		}
		
		public virtual float ComputeRangeIntensity(bool useRange, float rangeDistance, bool useRangeFalloff, AnimationCurve rangeFalloff, Vector2 remapRangeFalloff, Vector3 rangePosition)
		{
			if (!useRange)
			{
				return 1f;
			}

			float distanceToCenter = Vector3.Distance(rangePosition, this.transform.position);

			if (distanceToCenter > rangeDistance)
			{
				return 0f;
			}

			if (!useRangeFalloff)
			{
				return 1f;
			}

			float normalizedDistance = MMMaths.Remap(distanceToCenter, 0f, rangeDistance, 0f, 1f);
			float curveValue = rangeFalloff.Evaluate(normalizedDistance);
			float newIntensity = MMMaths.Remap(curveValue, 0f, 1f, remapRangeFalloff.x, remapRangeFalloff.y);
			return newIntensity;
		}
	}
}