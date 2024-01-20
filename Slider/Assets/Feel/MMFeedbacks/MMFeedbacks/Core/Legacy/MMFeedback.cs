using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A base class, meant to be extended, defining a Feedback. A Feedback is an action triggered by a MMFeedbacks, usually in reaction to the player's input or actions,
	/// to help communicate both emotion and legibility, improving game feel.
	/// To create a new feedback, extend this class and override its Custom methods, declared at the end of this class. You can look at the many examples for reference.
	/// </summary>
	[AddComponentMenu("")]
	[System.Serializable]
	[ExecuteAlways]
	public abstract class MMFeedback : MonoBehaviour
	{
		/// whether or not this feedback is active
		[Tooltip("whether or not this feedback is active")]
		public bool Active = true;
		/// the name of this feedback to display in the inspector
		[Tooltip("the name of this feedback to display in the inspector")]
		public string Label = "MMFeedback";
		/// the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
		[Tooltip("the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)")]
		[Range(0,100)]
		public float Chance = 100f;
		/// a number of timing-related values (delay, repeat, etc)
		[Tooltip("a number of timing-related values (delay, repeat, etc)")]
		public MMFeedbackTiming Timing;
		/// the Owner of the feedback, as defined when calling the Initialization method
		public GameObject Owner { get; set; }
		[HideInInspector]
		/// whether or not this feedback is in debug mode
		public bool DebugActive = false;
		/// set this to true if your feedback should pause the execution of the feedback sequence
		public virtual IEnumerator Pause { get { return null; } }
		/// if this is true, this feedback will wait until all previous feedbacks have run
		public virtual bool HoldingPause { get { return false; } }
		/// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
		public virtual bool LooperPause { get { return false; } }
		/// if this is true, this feedback will pause and wait until Resume() is called on its parent MMFeedbacks to resume execution
		public virtual bool ScriptDrivenPause { get; set; }
		/// if this is a positive value, the feedback will auto resume after that duration if it hasn't been resumed via script already
		public virtual float ScriptDrivenPauseAutoResume { get; set; }
		/// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
		public virtual bool LooperStart { get { return false; } }
		/// an overridable color for your feedback, that can be redefined per feedback. White is the only reserved color, and the feedback will revert to 
		/// normal (light or dark skin) when left to White
		#if UNITY_EDITOR
		public virtual Color FeedbackColor { get { return Color.white;  } }
		#endif
		/// returns true if this feedback is in cooldown at this time (and thus can't play), false otherwise
		public virtual bool InCooldown { get { return (Timing.CooldownDuration > 0f) && (FeedbackTime - _lastPlayTimestamp < Timing.CooldownDuration); } }
		/// if this is true, this feedback is currently playing
		public virtual bool IsPlaying { get; set; }
        
		/// the time (or unscaled time) based on the selected Timing settings
		public float FeedbackTime 
		{ 
			get 
			{
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.time;
				}
				else
				{
					return Time.unscaledTime;
				}
			} 
		}
        
		/// the delta time (or unscaled delta time) based on the selected Timing settings
		public float FeedbackDeltaTime
		{
			get
			{
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.deltaTime;
				}
				else
				{
					return Time.unscaledDeltaTime;
				}
			}
		}

        
		/// <summary>
		/// The total duration of this feedback :
		/// total = initial delay + duration * (number of repeats + delay between repeats)  
		/// </summary>
		public float TotalDuration
		{
			get
			{
				if ((Timing != null) && (!Timing.ContributeToTotalDuration))
				{
					return 0f;
				}
                
				float totalTime = 0f;

				if (Timing == null)
				{
					return 0f;
				}
                
				if (Timing.InitialDelay != 0)
				{
					totalTime += ApplyTimeMultiplier(Timing.InitialDelay);
				}
            
				totalTime += FeedbackDuration;

				if (Timing.NumberOfRepeats > 0)
				{
					float delayBetweenRepeats = ApplyTimeMultiplier(Timing.DelayBetweenRepeats); 
                    
					totalTime += (Timing.NumberOfRepeats * FeedbackDuration) + (Timing.NumberOfRepeats  * delayBetweenRepeats);
				}

				return totalTime;
			}
		}

		// the timestamp at which this feedback was last played
		public virtual float FeedbackStartedAt { get { return _lastPlayTimestamp; } }
		// the perceived duration of the feedback, to be used to display its progress bar, meant to be overridden with meaningful data by each feedback
		public virtual float FeedbackDuration { get { return 0f; } set { } }
		/// whether or not this feedback is playing right now
		public virtual bool FeedbackPlaying { get { return ((FeedbackStartedAt > 0f) && (Time.time - FeedbackStartedAt < FeedbackDuration)); } }

		public virtual MMChannelData ChannelData(int channel) => _channelData.Set(MMChannelModes.Int, channel, null);

		protected float _lastPlayTimestamp = -1f;
		protected int _playsLeft;
		protected bool _initialized = false;
		protected Coroutine _playCoroutine;
		protected Coroutine _infinitePlayCoroutine;
		protected Coroutine _sequenceCoroutine;
		protected Coroutine _repeatedPlayCoroutine;
		protected int _sequenceTrackID = 0;
		protected MMFeedbacks _hostMMFeedbacks;

		protected float _beatInterval;
		protected bool BeatThisFrame = false;
		protected int LastBeatIndex = 0;
		protected int CurrentSequenceIndex = 0;
		protected float LastBeatTimestamp = 0f;
		protected bool _isHostMMFeedbacksNotNull;
		protected MMChannelData _channelData;

		protected virtual void OnEnable()
		{
			_hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
			_isHostMMFeedbacksNotNull = _hostMMFeedbacks != null;
		}

		/// <summary>
		/// Initializes the feedback and its timing related variables
		/// </summary>
		/// <param name="owner"></param>
		public virtual void Initialization(GameObject owner)
		{
			_initialized = true;
			Owner = owner;
			_playsLeft = Timing.NumberOfRepeats + 1;
			_hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
			_channelData = new MMChannelData(MMChannelModes.Int, 0, null);
            
			SetInitialDelay(Timing.InitialDelay);
			SetDelayBetweenRepeats(Timing.DelayBetweenRepeats);
			SetSequence(Timing.Sequence);

			CustomInitialization(owner);            
		}
        
		/// <summary>
		/// Plays the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void Play(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active)
			{
				return;
			}

			if (!_initialized)
			{
				Debug.LogWarning("The " + this + " feedback is being played without having been initialized. Call Initialization() first.");
			}
            
			// we check the cooldown
			if (InCooldown)
			{
				return;
			}

			if (Timing.InitialDelay > 0f) 
			{
				_playCoroutine = StartCoroutine(PlayCoroutine(position, feedbacksIntensity));
			}
			else
			{
				_lastPlayTimestamp = FeedbackTime;
				RegularPlay(position, feedbacksIntensity);
			}  
		}
        
		/// <summary>
		/// An internal coroutine delaying the initial play of the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator PlayCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Timing.TimescaleMode == TimescaleModes.Scaled)
			{
				yield return MMFeedbacksCoroutine.WaitFor(Timing.InitialDelay);
			}
			else
			{
				yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.InitialDelay);
			}
			_lastPlayTimestamp = FeedbackTime;
			RegularPlay(position, feedbacksIntensity);
		}

		/// <summary>
		/// Triggers delaying coroutines if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Chance == 0f)
			{
				return;
			}
			if (Chance != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > Chance)
				{
					return;
				}
			}

			if (Timing.UseIntensityInterval)
			{
				if ((feedbacksIntensity < Timing.IntensityIntervalMin) || (feedbacksIntensity >= Timing.IntensityIntervalMax))
				{
					return;
				}
			}

			if (Timing.RepeatForever)
			{
				_infinitePlayCoroutine = StartCoroutine(InfinitePlay(position, feedbacksIntensity));
				return;
			}
			if (Timing.NumberOfRepeats > 0)
			{
				_repeatedPlayCoroutine = StartCoroutine(RepeatedPlay(position, feedbacksIntensity));
				return;
			}            
			if (Timing.Sequence == null)
			{
				CustomPlayFeedback(position, feedbacksIntensity);
			}
			else
			{
				_sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
			}
            
		}

		/// <summary>
		/// Internal coroutine used for repeated play without end
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator InfinitePlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (true)
			{
				_lastPlayTimestamp = FeedbackTime;
				if (Timing.Sequence == null)
				{
					CustomPlayFeedback(position, feedbacksIntensity);
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
					}
				}
				else
				{
					_sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));

					float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(delay);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
					}
				}
			}
		}

		/// <summary>
		/// Internal coroutine used for repeated play
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator RepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (_playsLeft > 0)
			{
				_lastPlayTimestamp = FeedbackTime;
				_playsLeft--;
				if (Timing.Sequence == null)
				{
					CustomPlayFeedback(position, feedbacksIntensity);
                    
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
					}
				}
				else
				{
					_sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
                    
					float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(delay);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
					}
				}
			}
			_playsLeft = Timing.NumberOfRepeats + 1;
		}

		/// <summary>
		/// A coroutine used to play this feedback on a sequence
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator SequenceCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			yield return null;
			float timeStartedAt = FeedbackTime;
			float lastFrame = FeedbackTime;

			BeatThisFrame = false;
			LastBeatIndex = 0;
			CurrentSequenceIndex = 0;
			LastBeatTimestamp = 0f;

			if (Timing.Quantized)
			{
				while (CurrentSequenceIndex < Timing.Sequence.QuantizedSequence[0].Line.Count)
				{
					_beatInterval = 60f / Timing.TargetBPM;

					if ((FeedbackTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
					{
						BeatThisFrame = true;
						LastBeatIndex = CurrentSequenceIndex;
						LastBeatTimestamp = FeedbackTime;

						for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
						{
							if (Timing.Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID == Timing.TrackID)
							{
								CustomPlayFeedback(position, feedbacksIntensity);
							}
						}
						CurrentSequenceIndex++;
					}
					yield return null;
				}
			}
			else
			{
				while (FeedbackTime - timeStartedAt < Timing.Sequence.Length)
				{
					foreach (MMSequenceNote item in Timing.Sequence.OriginalSequence.Line)
					{
						if ((item.ID == Timing.TrackID) && (item.Timestamp >= lastFrame) && (item.Timestamp <= FeedbackTime - timeStartedAt))
						{
							CustomPlayFeedback(position, feedbacksIntensity);
						}
					}
					lastFrame = FeedbackTime - timeStartedAt;
					yield return null;
				}
			}
                    
		}

		/// <summary>
		/// Stops all feedbacks from playing. Will stop repeating feedbacks, and call custom stop implementations
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (_playCoroutine != null) { StopCoroutine(_playCoroutine); }
			if (_infinitePlayCoroutine != null) { StopCoroutine(_infinitePlayCoroutine); }
			if (_repeatedPlayCoroutine != null) { StopCoroutine(_repeatedPlayCoroutine); }            
			if (_sequenceCoroutine != null) { StopCoroutine(_sequenceCoroutine);  }

			_lastPlayTimestamp = 0f;
			_playsLeft = Timing.NumberOfRepeats + 1;
			if (Timing.InterruptsOnStop)
			{
				CustomStopFeedback(position, feedbacksIntensity);    
			}
		}

		/// <summary>
		/// Calls this feedback's custom reset 
		/// </summary>
		public virtual void ResetFeedback()
		{
			_playsLeft = Timing.NumberOfRepeats + 1;
			CustomReset();
		}

		/// <summary>
		/// Use this method to change this feedback's sequence at runtime
		/// </summary>
		/// <param name="newSequence"></param>
		public virtual void SetSequence(MMSequence newSequence)
		{
			Timing.Sequence = newSequence;
			if (Timing.Sequence != null)
			{
				for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
				{
					if (Timing.Sequence.SequenceTracks[i].ID == Timing.TrackID)
					{
						_sequenceTrackID = i;
					}
				}
			}
		}

		/// <summary>
		/// Use this method to specify a new delay between repeats at runtime
		/// </summary>
		/// <param name="delay"></param>
		public virtual void SetDelayBetweenRepeats(float delay)
		{
			Timing.DelayBetweenRepeats = delay;
		}

		/// <summary>
		/// Use this method to specify a new initial delay at runtime
		/// </summary>
		/// <param name="delay"></param>
		public virtual void SetInitialDelay(float delay)
		{
			Timing.InitialDelay = delay;
		}

		/// <summary>
		/// Returns a new value of the normalized time based on the current play direction of this feedback
		/// </summary>
		/// <param name="normalizedTime"></param>
		/// <returns></returns>
		protected virtual float ApplyDirection(float normalizedTime)
		{
			return NormalPlayDirection ? normalizedTime : 1 - normalizedTime;
		}

		/// <summary>
		/// Returns true if this feedback should play normally, or false if it should play in rewind
		/// </summary>
		public virtual bool NormalPlayDirection
		{
			get
			{
				switch (Timing.PlayDirection)
				{
					case MMFeedbackTiming.PlayDirections.FollowMMFeedbacksDirection:
						return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
					case MMFeedbackTiming.PlayDirections.AlwaysNormal:
						return true;
					case MMFeedbackTiming.PlayDirections.AlwaysRewind:
						return false;
					case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
						return !(_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
				}
				return true;
			}
		}

		/// <summary>
		/// Returns true if this feedback should play in the current parent MMFeedbacks direction, according to its MMFeedbacksDirectionCondition setting
		/// </summary>
		public virtual bool ShouldPlayInThisSequenceDirection
		{
			get
			{
				switch (Timing.MMFeedbacksDirectionCondition)
				{
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.Always:
						return true;
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards:
						return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
						return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop);
				}
				return true;
			}
		}

		/// <summary>
		/// Returns the t value at which to evaluate a curve at the end of this feedback's play time
		/// </summary>
		protected virtual float FinalNormalizedTime
		{
			get
			{
				return NormalPlayDirection ? 1f : 0f;
			}
		}

		/// <summary>
		/// Applies the host MMFeedbacks' time multiplier to this feedback
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected virtual float ApplyTimeMultiplier(float duration)
		{
			if (_isHostMMFeedbacksNotNull)
			{
				return _hostMMFeedbacks.ApplyTimeMultiplier(duration);    
			}

			return duration;
		}

		/// <summary>
		/// This method describes all custom initialization processes the feedback requires, in addition to the main Initialization method
		/// </summary>
		/// <param name="owner"></param>
		protected virtual void CustomInitialization(GameObject owner) { }

		/// <summary>
		/// This method describes what happens when the feedback gets played
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected abstract void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f);

		/// <summary>
		/// This method describes what happens when the feedback gets stopped
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }

		/// <summary>
		/// This method describes what happens when the feedback gets reset
		/// </summary>
		protected virtual void CustomReset() { }
	}   
}