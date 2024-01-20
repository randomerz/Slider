using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using MoreMountains.Tools;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A collection of MMFeedback, meant to be played altogether.
	/// This class provides a custom inspector to add and customize feedbacks, and public methods to trigger them, stop them, etc.
	/// You can either use it on its own, or bind it from another class and trigger it from there.
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/MMFeedbacks")]
	public class MMFeedbacks : MonoBehaviour
	{
		/// the possible directions MMFeedbacks can be played
		public enum Directions { TopToBottom, BottomToTop }
		/// the possible SafeModes (will perform checks to make sure no serialization error has damaged them)
		/// - nope : no safety
		/// - editor only : performs checks on enable
		/// - runtime only : performs checks on Awake
		/// - full : performs both editor and runtime checks, recommended setting
		public enum SafeModes { Nope, EditorOnly, RuntimeOnly, Full }
        
		/// a list of MMFeedback to trigger
		public List<MMFeedback> Feedbacks = new List<MMFeedback>();
        
		/// the possible initialization modes. If you use Script, you'll have to initialize manually by calling the Initialization method and passing it an owner
		/// Otherwise, you can have this component initialize itself at Awake or Start, and in this case the owner will be the MMFeedbacks itself
		public enum InitializationModes { Script, Awake, Start }
		/// the chosen initialization mode
		[Tooltip("the chosen initialization modes. If you use Script, you'll have to initialize manually by calling the " +
		         "Initialization method and passing it an owner. Otherwise, you can have this component initialize " +
		         "itself at Awake or Start, and in this case the owner will be the MMFeedbacks itself")]
		public InitializationModes InitializationMode = InitializationModes.Start;
		/// if you set this to true, the system will make changes to ensure that initialization always happens before play
		[Tooltip("if you set this to true, the system will make changes to ensure that initialization always happens before play")]
		public bool AutoInitialization = true;
		/// the selected safe mode
		[Tooltip("the selected safe mode")]
		public SafeModes SafeMode = SafeModes.Full;
		/// the selected direction
		[Tooltip("the selected direction these feedbacks should play in")]
		public Directions Direction = Directions.TopToBottom;
		/// whether or not this MMFeedbacks should invert its direction when all feedbacks have played
		[Tooltip("whether or not this MMFeedbacks should invert its direction when all feedbacks have played")]
		public bool AutoChangeDirectionOnEnd = false;
		/// whether or not to play this feedbacks automatically on Start
		[Tooltip("whether or not to play this feedbacks automatically on Start")]
		public bool AutoPlayOnStart = false;
		/// whether or not to play this feedbacks automatically on Enable
		[Tooltip("whether or not to play this feedbacks automatically on Enable")]
		public bool AutoPlayOnEnable = false;

		/// if this is true, all feedbacks within that player will work on the specified ForcedTimescaleMode, regardless of their individual settings 
		[Tooltip("if this is true, all feedbacks within that player will work on the specified ForcedTimescaleMode, regardless of their individual settings")] 
		public bool ForceTimescaleMode = false;
		/// the time scale mode all feedbacks on this player should work on, if ForceTimescaleMode is true
		[Tooltip("the time scale mode all feedbacks on this player should work on, if ForceTimescaleMode is true")] 
		[MMFCondition("ForceTimescaleMode", true)]
		public TimescaleModes ForcedTimescaleMode = TimescaleModes.Unscaled;
		/// a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)
		[Tooltip("a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)")]
		public float DurationMultiplier = 1f;
		/// if this is true, will expose a RandomDurationMultiplier. The final duration of each feedback will be : their base duration * DurationMultiplier * a random value between RandomDurationMultiplier.x and RandomDurationMultiplier.y
		[Tooltip("if this is true, will expose a RandomDurationMultiplier. The final duration of each feedback will be : their base duration * DurationMultiplier * a random value between RandomDurationMultiplier.x and RandomDurationMultiplier.y")]
		public bool RandomizeDuration = false;
		/// if RandomizeDuration is true, the min (x) and max (y) values for the random duration multiplier
		[Tooltip("if RandomizeDuration is true, the min (x) and max (y) values for the random duration multiplier")]
		[MMCondition("RandomizeDuration", true)]
		public Vector2 RandomDurationMultiplier = new Vector2(0.5f, 1.5f);
		/// if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot
		[Tooltip("if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot")]
		public bool DisplayFullDurationDetails = false;
		/// the timescale at which the player itself will operate. This notably impacts sequencing and pauses duration evaluation.
		[Tooltip("the timescale at which the player itself will operate. This notably impacts sequencing and pauses duration evaluation.")]
		public TimescaleModes PlayerTimescaleMode = TimescaleModes.Unscaled;

		/// if this is true, this feedback will only play if its distance to RangeCenter is lower or equal to RangeDistance
		[Tooltip("if this is true, this feedback will only play if its distance to RangeCenter is lower or equal to RangeDistance")]
		public bool OnlyPlayIfWithinRange = false;
		/// when in OnlyPlayIfWithinRange mode, the transform to consider as the center of the range
		[Tooltip("when in OnlyPlayIfWithinRange mode, the transform to consider as the center of the range")]
		public Transform RangeCenter;
		/// when in OnlyPlayIfWithinRange mode, the distance to the center within which the feedback will play
		[Tooltip("when in OnlyPlayIfWithinRange mode, the distance to the center within which the feedback will play")]
		public float RangeDistance = 5f;
		/// when in OnlyPlayIfWithinRange mode, whether or not to modify the intensity of feedbacks based on the RangeFallOff curve  
		[Tooltip("when in OnlyPlayIfWithinRange mode, whether or not to modify the intensity of feedbacks based on the RangeFallOff curve")]
		public bool UseRangeFalloff = false;
		/// the animation curve to use to define falloff (on the x 0 represents the range center, 1 represents the max distance to it)
		[Tooltip("the animation curve to use to define falloff (on the x 0 represents the range center, 1 represents the max distance to it)")]
		[MMFCondition("UseRangeFalloff", true)]
		public AnimationCurve RangeFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		/// the values to remap the falloff curve's y axis' 0 and 1
		[Tooltip("the values to remap the falloff curve's y axis' 0 and 1")]
		[MMFVector("Zero","One")]
		public Vector2 RemapRangeFalloff = new Vector2(0f, 1f);
		/// whether or not to ignore MMSetFeedbackRangeCenterEvent, used to set the RangeCenter from anywhere
		[Tooltip("whether or not to ignore MMSetFeedbackRangeCenterEvent, used to set the RangeCenter from anywhere")]
		public bool IgnoreRangeEvents = false;

		/// a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible
		[Tooltip("a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible")]
		public float CooldownDuration = 0f;
		/// a duration, in seconds, to delay the start of this MMFeedbacks' contents play
		[Tooltip("a duration, in seconds, to delay the start of this MMFeedbacks' contents play")]
		public float InitialDelay = 0f;
		/// whether this player can be played or not, useful to temporarily prevent play from another class, for example
		[Tooltip("whether this player can be played or not, useful to temporarily prevent play from another class, for example")]
		public bool CanPlay = true;
		/// if this is true, you'll be able to trigger a new Play while this feedback is already playing, otherwise you won't be able to
		[Tooltip("if this is true, you'll be able to trigger a new Play while this feedback is already playing, otherwise you won't be able to")]
		public bool CanPlayWhileAlreadyPlaying = true;
		/// the chance of this sequence happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
		[Tooltip("the chance of this sequence happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)")]
		[Range(0,100)]
		public float ChanceToPlay = 100f;
        
		/// the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect.
		/// Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.  
		[Tooltip("the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect." +
		         "Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.")]
		public float FeedbacksIntensity = 1f;

		/// a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks 
		[Tooltip("a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks")] 
		public MMFeedbacksEvents Events;
        
		/// a global switch used to turn all feedbacks on or off globally
		[Tooltip("a global switch used to turn all feedbacks on or off globally")]
		public static bool GlobalMMFeedbacksActive = true;
        
		[HideInInspector]
		/// whether or not this MMFeedbacks is in debug mode
		public bool DebugActive = false;
		/// whether or not this MMFeedbacks is playing right now - meaning it hasn't been stopped yet.
		/// if you don't stop your MMFeedbacks it'll remain true of course
		public bool IsPlaying { get; protected set; }
		/// if this MMFeedbacks is playing the time since it started playing
		public float ElapsedTime => IsPlaying ? GetTime() - _lastStartAt : 0f;
		/// the amount of times this MMFeedbacks has been played
		public int TimesPlayed { get; protected set; }
		/// whether or not the execution of this MMFeedbacks' sequence is being prevented and waiting for a Resume() call
		public bool InScriptDrivenPause { get; set; }
		/// true if this MMFeedbacks contains at least one loop
		public bool ContainsLoop { get; set; }
		/// true if this feedback should change play direction next time it's played
		public bool ShouldRevertOnNextPlay { get; set; }
		/// true if this player is forcing unscaled mode
		public bool ForcingUnscaledTimescaleMode { get { return (ForceTimescaleMode && ForcedTimescaleMode == TimescaleModes.Unscaled);  } }
		/// The total duration (in seconds) of all the active feedbacks in this MMFeedbacks
		public virtual float TotalDuration
		{
			get
			{
				float total = 0f;
				foreach (MMFeedback feedback in Feedbacks)
				{
					if ((feedback != null) && (feedback.Active))
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
				return InitialDelay + total;
			}
		}
        
		public virtual float GetTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		protected float _startTime = 0f;
		protected float _holdingMax = 0f;
		protected float _lastStartAt = -float.MaxValue;
		protected int _lastStartFrame = -1;
		protected bool _pauseFound = false;
		protected float _totalDuration = 0f;
		protected bool _shouldStop = false;
		protected const float _smallValue = 0.001f;
		protected float _randomDurationMultiplier = 1f;
		protected float _lastOnEnableFrame = -1;

		#region INITIALIZATION

		/// <summary>
		/// On Awake we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void Awake()
		{
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMFeedbacksEnabler enabler = GetComponent<MMFeedbacksEnabler>(); 
				if (enabler == null)
				{
					enabler = this.gameObject.AddComponent<MMFeedbacksEnabler>();
				}
				enabler.TargetMMFeedbacks = this;
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			CheckForLoops();
		}

		/// <summary>
		/// On Start we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

		/// <summary>
		/// On Enable we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void OnEnable()
		{
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				PlayFeedbacks();
			}
		}

		/// <summary>
		/// Initializes the MMFeedbacks, setting this MMFeedbacks as the owner
		/// </summary>
		public virtual void Initialization(bool forceInitIfPlaying = false)
		{
			Initialization(this.gameObject);
		}

		/// <summary>
		/// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="feedbacksOwner"></param>
		public virtual void Initialization(GameObject owner)
		{
			if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
			{
				AutoRepair();
			}

			IsPlaying = false;
			TimesPlayed = 0;
			_lastStartAt = -float.MaxValue;

			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					Feedbacks[i].Initialization(owner);
				}                
			}
		}

		#endregion

		#region PLAY
        
		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
		/// </summary>
		public virtual void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
		}
        
		/// <summary>
		/// Plays all feedbacks and awaits until completion
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		public virtual async System.Threading.Tasks.Task PlayFeedbacksTask(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks and awaits until completion
		/// </summary>
		public virtual async System.Threading.Tasks.Task PlayFeedbacksTask()
		{
			PlayFeedbacks();
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks, specifying a position and intensity. The position may be used by each Feedback and taken into account to spark a particle or play a sound for example.
		/// The feedbacks intensity is a factor that can be used by each Feedback to lower its intensity, usually you'll want to define that attenuation based on time or distance (using a lower 
		/// intensity value for feedbacks happening further away from the Player).
		/// Additionally you can force the feedback to play in reverse, ignoring its current condition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksOwner"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public virtual void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public virtual void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfReversed()
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

		/// <summary>
		/// A public coroutine you can call externally when you want to yield in a coroutine of yours until the MMFeedbacks has stopped playing
		/// typically : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
		/// </summary>
		/// <param name="position">The position at which the MMFeedbacks should play</param>
		/// <param name="feedbacksIntensity">The intensity of the feedback</param>
		/// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
		/// <returns></returns>
		public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				yield return null;    
			}
		}

		#endregion

		#region SEQUENCE

		/// <summary>
		/// An internal method used to play feedbacks, shouldn't be called externally
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			if (!CanPlay)
			{
				return;
			}
			
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
				return;
			}

			if (!EvaluateChance())
			{
				return;
			}

			// if we have a cooldown we prevent execution if needed
			if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
					return;
				}
			}

			// if all MMFeedbacks are disabled globally, we stop and don't play
			if (!GlobalMMFeedbacksActive)
			{
				return;
			}

			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}
            
			if (ShouldRevertOnNextPlay)
			{
				Revert();
				ShouldRevertOnNextPlay = false;
			}

			if (forceRevert)
			{
				Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
			}
            
			ResetFeedbacks();
			this.enabled = true;
			TimesPlayed++;
			IsPlaying = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			_totalDuration = TotalDuration;
			CheckForPauses();
            
			if (InitialDelay > 0f)
			{
				StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}

		protected virtual void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			Events.TriggerOnPlay(this);

			_holdingMax = 0f;
			CheckForPauses();
			
			if (!_pauseFound)
			{
				PlayAllFeedbacks(position, feedbacksIntensity, forceRevert);
			}
			else
			{
				// if at least one pause was found
				StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
			}
		}

		protected virtual void CheckForPauses()
		{
			_pauseFound = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
		}

		protected virtual void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			// if no pause was found, we just play all feedbacks at once
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (FeedbackCanPlay(Feedbacks[i]))
				{
					Feedbacks[i].Play(position, feedbacksIntensity);
				}
			}
		}

		protected virtual IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitFor(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected virtual void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				Events.TriggerOnComplete(this);
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > _totalDuration)
					{
						_shouldStop = true;
					}    
				}
			}
			else
			{
				this.enabled = false;
			}
		}

		/// <summary>
		/// Returns true if feedbacks are still playing
		/// </summary>
		/// <returns></returns>
		public virtual bool HasFeedbackStillPlaying()
		{
			int count = Feedbacks.Count;
			for (int i = 0; i < count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].IsPlaying))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// A coroutine used to handle the sequence of feedbacks if pauses are involved
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			IsPlaying = true;

			int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count-1;

			while ((i >= 0) && (i < Feedbacks.Count))
			{
				if (!IsPlaying)
				{
					yield break;
				}

				if (Feedbacks[i] == null)
				{
					yield break;
				}
                
				if (((Feedbacks[i].Active) && (Feedbacks[i].ScriptDrivenPause)) || InScriptDrivenPause)
				{
					InScriptDrivenPause = true;

					bool inAutoResume = (Feedbacks[i].ScriptDrivenPauseAutoResume > 0f); 
					float scriptDrivenPauseStartedAt = GetTime();
					float autoResumeDuration = Feedbacks[i].ScriptDrivenPauseAutoResume;
                    
					while (InScriptDrivenPause)
					{
						if (inAutoResume && (GetTime() - scriptDrivenPauseStartedAt > autoResumeDuration))
						{
							ResumeFeedbacks();
						}
						yield return null;
					} 
				}

				// handles holding pauses
				if ((Feedbacks[i].Active)
				    && ((Feedbacks[i].HoldingPause == true) || (Feedbacks[i].LooperPause == true))
				    && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
				{
					Events.TriggerOnPause(this);
					// we stay here until all previous feedbacks have finished
					while (GetTime() - _lastStartAt < _holdingMax)
					{
						yield return null;
					}
					_holdingMax = 0f;
					_lastStartAt = GetTime();
				}

				// plays the feedback
				if (FeedbackCanPlay(Feedbacks[i]))
				{
					Feedbacks[i].Play(position, feedbacksIntensity);
				}

				// Handles pause
				if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
				{
					bool shouldPause = true;
					if (Feedbacks[i].Chance < 100)
					{
						float random = Random.Range(0f, 100f);
						if (random > Feedbacks[i].Chance)
						{
							shouldPause = false;
						}
					}

					if (shouldPause)
					{
						yield return Feedbacks[i].Pause;
						Events.TriggerOnResume(this);
						_lastStartAt = GetTime();
						_holdingMax = 0f;
					}
				}

				// updates holding max
				if (Feedbacks[i].Active)
				{
					if ((Feedbacks[i].Pause == null) && (Feedbacks[i].ShouldPlayInThisSequenceDirection) && (!Feedbacks[i].Timing.ExcludeFromHoldingPauses))
					{
						float feedbackDuration = Feedbacks[i].TotalDuration;
						_holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
					}
				}

				// handles looper
				if ((Feedbacks[i].LooperPause == true)
				    && (Feedbacks[i].Active)
				    && (Feedbacks[i].ShouldPlayInThisSequenceDirection)
				    && (((Feedbacks[i] as MMFeedbackLooper).NumberOfLoopsLeft > 0) || (Feedbacks[i] as MMFeedbackLooper).InInfiniteLoop))
				{
					// we determine the index we should start again at
					bool loopAtLastPause = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastPause;
					bool loopAtLastLoopStart = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastLoopStart;

					int newi = 0;

					int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

					while ((j >= 0) && (j <= Feedbacks.Count))
					{
						// if we're at the start
						if (j == 0)
						{
							newi = j - 1;
							break;
						}
						if (j == Feedbacks.Count)
						{
							newi = j ;
							break;
						}
						// if we've found a pause
						if ((Feedbacks[j].Pause != null)
						    && (Feedbacks[j].FeedbackDuration > 0f)
						    && loopAtLastPause && (Feedbacks[j].Active))
						{
							newi = j;
							break;
						}
						// if we've found a looper start
						if ((Feedbacks[j].LooperStart == true)
						    && loopAtLastLoopStart
						    && (Feedbacks[j].Active))
						{
							newi = j;
							break;
						}

						j += (Direction == Directions.TopToBottom) ? -1 : 1;
					}
					i = newi;
				}
				i += (Direction == Directions.TopToBottom) ? 1 : -1;
			}
			float unscaledTimeAtEnd = GetTime();
			while (GetTime() - unscaledTimeAtEnd < _holdingMax)
			{
				yield return null;
			}
            
			while (HasFeedbackStillPlaying())
			{
				yield return null;
			}
            
			IsPlaying = false;
			Events.TriggerOnComplete(this);
			ApplyAutoRevert();
		}

		#endregion

		#region STOP

		/// <summary>
		/// Stops all further feedbacks from playing, without stopping individual feedbacks 
		/// </summary>
		public virtual void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

		/// <summary>
		/// Stops all feedbacks from playing, with an option to also stop individual feedbacks
		/// </summary>
		public virtual void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

		/// <summary>
		/// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				for (int i = 0; i < Feedbacks.Count; i++)
				{
					if (Feedbacks[i] != null)
					{
						Feedbacks[i].Stop(position, feedbacksIntensity);	
					}
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}
        
		#endregion 

		#region CONTROLS

		/// <summary>
		/// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer.
		/// </summary>
		public virtual void ResetFeedbacks()
		{
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
				{
					Feedbacks[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

		/// <summary>
		/// Changes the direction of this MMFeedbacks
		/// </summary>
		public virtual void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

		/// <summary>
		/// Use this method to authorize or prevent this player from being played
		/// </summary>
		/// <param name="newState"></param>
		public virtual void SetCanPlay(bool newState)
		{
			CanPlay = newState;
		}

		/// <summary>
		/// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
		/// </summary>
		public virtual void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

		/// <summary>
		/// Resumes execution of a sequence if a script driven pause is in progress
		/// </summary>
		public virtual void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

		#endregion
        
		#region MODIFICATION
        
		public virtual MMFeedback AddFeedback(System.Type feedbackType, bool add = true)
		{
			MMFeedback newFeedback;
            
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				newFeedback = Undo.AddComponent(this.gameObject, feedbackType) as MMFeedback;
			}
			else
			{
				newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			}
			#else 
                newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			#endif
            
			newFeedback.hideFlags = HideFlags.HideInInspector;
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);

			AutoRepair();
            
			return newFeedback;
		}
        
		public virtual void RemoveFeedback(int id)
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Undo.DestroyObjectImmediate(Feedbacks[id]);
			}
			else
			{
				DestroyImmediate(Feedbacks[id]);
			}
			#else
                DestroyImmediate(Feedbacks[id]);
			#endif
            
			Feedbacks.RemoveAt(id);
			AutoRepair();
		}
        
		#endregion MODIFICATION

		#region HELPERS

		/// <summary>
		/// Evaluates the chance of this feedback to play, and returns true if this feedback can play, false otherwise
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateChance()
		{
			if (ChanceToPlay == 0f)
			{
				return false;
			}
			if (ChanceToPlay != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > ChanceToPlay)
				{
					return false;
				}
			}

			return true;
		}
        
		/// <summary>
		/// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
		/// </summary>
		protected virtual void CheckForLoops()
		{
			ContainsLoop = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}
        
		/// <summary>
		/// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		protected bool FeedbackCanPlay(MMFeedback feedback)
		{
			if (feedback == null)
			{
				return false;
			}
			
			if (feedback.Timing == null)
			{
				return false;
			}
			
			if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
			{
				return true;
			}
			else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
			         || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Readies the MMFeedbacks to revert direction on the next play
		/// </summary>
		protected virtual void ApplyAutoRevert()
		{
			if (AutoChangeDirectionOnEnd)
			{
				ShouldRevertOnNextPlay = true;
			}
		}
        
		/// <summary>
		/// Applies this feedback's time multiplier to a duration (in seconds)
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public virtual float ApplyTimeMultiplier(float duration)
		{
			return duration * Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

		/// <summary>
		/// Unity sometimes has serialization issues. 
		/// This method fixes that by fixing any bad sync that could happen.
		/// </summary>
		public virtual void AutoRepair()
		{
			List<Component> components = components = new List<Component>();
			components = this.gameObject.GetComponents<Component>().ToList();
			foreach (Component component in components)
			{
				if (component is MMFeedback)
				{
					bool found = false;
					for (int i = 0; i < Feedbacks.Count; i++)
					{
						if (Feedbacks[i] == (MMFeedback)component)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						Feedbacks.Add((MMFeedback)component);
					}
				}
			}
		} 

		#endregion 
        
		#region EVENTS

		/// <summary>
		/// On Disable we stop all feedbacks
		/// </summary>
		protected virtual void OnDisable()
		{
			/*if (IsPlaying)
			{
			    StopFeedbacks();
			    StopAllCoroutines();
			}*/
		}

		/// <summary>
		/// On validate, we make sure our DurationMultiplier remains positive
		/// </summary>
		protected virtual void OnValidate()
		{
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

		/// <summary>
		/// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
		/// </summary>
		protected virtual void OnDestroy()
		{
			IsPlaying = false;
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{            
				// we remove all binders
				foreach (MMFeedback feedback in Feedbacks)
				{
					EditorApplication.delayCall += () =>
					{
						DestroyImmediate(feedback);
					};                    
				}
			}
			#endif
		}     
        
		#endregion EVENTS
	}
}