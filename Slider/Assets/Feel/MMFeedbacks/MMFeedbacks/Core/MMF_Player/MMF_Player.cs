using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Feedbacks/MMF Player")]
	[DisallowMultipleComponent] 
	public class MMF_Player : MMFeedbacks
	{
		#region PROPERTIES
        
		[SerializeReference]
		public List<MMF_Feedback> FeedbacksList;
        
		public override float TotalDuration
		{
			get
			{
				return _cachedTotalDuration;
			}
		}

		public bool KeepPlayModeChanges = false;
		/// if this is true, the inspector won't refresh while the feedback plays, this saves on performance but feedback inspectors' progress bars for example won't look as smooth
		[Tooltip("if this is true, the inspector won't refresh while the feedback plays, this saves on performance but feedback inspectors' progress bars for example won't look as smooth")]
		public bool PerformanceMode = false;
		/// if this is true, StopFeedbacks will be called on all feedbacks on Disable
		[Tooltip("if this is true, StopFeedbacks will be called on all feedbacks on Disable")]
		public bool StopFeedbacksOnDisable = false;
		/// how many times this player has started playing
		[Tooltip("how many times this player has started playing")]
		[MMReadOnly]
		public int PlayCount = 0;

		public bool SkippingToTheEnd { get; protected set; }
        
		protected Type _t;
		protected float _cachedTotalDuration;
		protected bool _initialized = false;
        
		#endregion
        
		#region INITIALIZATION

		/// <summary>
		/// On Awake we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected override void Awake()
		{
			if (AutoInitialization && (AutoPlayOnEnable || AutoPlayOnStart))
			{
				InitializationMode = InitializationModes.Awake;
			}
			
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMF_PlayerEnabler playerEnabler = GetComponent<MMF_PlayerEnabler>(); 
				if (playerEnabler == null)
				{
					playerEnabler = this.gameObject.AddComponent<MMF_PlayerEnabler>();
				}
				playerEnabler.TargetMmfPlayer = this; 
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization();
			}

			InitializeFeedbackList();
			ExtraInitializationChecks();
			CheckForLoops();
			ComputeCachedTotalDuration();
			PreInitialization();
		}

		/// <summary>
		/// On Start we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected override void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization();
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

		/// <summary>
		/// We initialize our list of feedbacks
		/// </summary>
		protected virtual void InitializeFeedbackList()
		{
			if (FeedbacksList == null)
			{
				FeedbacksList = new List<MMF_Feedback>();
			}
		}

		/// <summary>
		/// Performs extra checks, mostly to cover cases of dynamic creation
		/// </summary>
		protected virtual void ExtraInitializationChecks()
		{
			if (Events == null)
			{
				Events = new MMFeedbacksEvents();
				Events.Initialization();
			}
		}

		/// <summary>
		/// On Enable we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected override void OnEnable()
		{
			if (OnlyPlayIfWithinRange)
			{
				MMSetFeedbackRangeCenterEvent.Register(OnMMSetFeedbackRangeCenterEvent);	
			}
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				feedback.CacheRequiresSetup();
			}
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				if (_lastOnEnableFrame == Time.frameCount)
				{
					return;
				}
				
				// if we're in the very first frames, we delay our play for 2 frames to avoid Unity bugs
				if (Time.frameCount < 2)
				{
					_lastOnEnableFrame = 2;
					StartCoroutine(PlayFeedbacksAfterFrames(2));
				}
				else
				{
					PlayFeedbacks();
				}
			}
		}

		/// <summary>
		/// A coroutine you can start to play this player's feedbacks after X frames
		/// </summary>
		/// <param name="framesAmount"></param>
		/// <returns></returns>
		public virtual IEnumerator PlayFeedbacksAfterFrames(int framesAmount)
		{
			yield return MMFeedbacksCoroutine.WaitForFrames(framesAmount);
			PlayFeedbacks();
		}

		public virtual void PreInitialization()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					FeedbacksList[i].PreInitialization(this, i);
				}                
			}
		}

		/// <summary>
		/// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="feedbacksOwner"></param>
		public override void Initialization(bool forceInitIfPlaying = false)
		{
			if (IsPlaying && !forceInitIfPlaying)
			{
				return;
			}
			
			SkippingToTheEnd = false;
			IsPlaying = false;
			_lastStartAt = -float.MaxValue;

			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					FeedbacksList[i].Initialization(this, i);
				}
			}

			_initialized = true;
		}

		/// <summary>
		/// When calling the legacy init method that used to specify an owner, we force the MMF Player init to run
		/// </summary>
		/// <param name="owner"></param>
		public override void Initialization(GameObject owner)
		{
			Initialization();
		}

		#endregion

		#region PLAY
        
		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
		/// </summary>
		public override void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
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
		public override void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public override void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public override void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public override void PlayFeedbacksOnlyIfReversed()
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
		public override void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
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
		public override void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public override void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
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
		public override IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
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
		protected override void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			if (AutoInitialization)
			{
				if (!_initialized)
				{
					Initialization();
				}
			}
			
			if (!IsAllowedToPlay(position))
			{
				return;
			}
            
			SkippingToTheEnd = false;
            
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
			_lastStartFrame = Time.frameCount;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			this.enabled = true;
			IsPlaying = true;
			PlayCount++;
			ComputeNewRandomDurationMultipliers();
			CheckForPauses();
            
			if (Time.frameCount < 2)
			{
				this.enabled = false;
				StartCoroutine(FrameOnePlayCo(position, feedbacksIntensity, forceRevert));
				return;
			}

			if (InitialDelay > 0f)
			{
				StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}

		/// <summary>
		/// Returns true if this feedback is allowed to play, false otherwise
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToPlay(Vector3 position)
		{
			// if CanPlay is false, we're not allowed to play
			if (!CanPlay)
			{
				return false;
			}
			
			// if we're already playing and can't play while already playing, we're not allowed to play 
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
				return false;
			}

			if (AutoPlayOnEnable && (_lastStartFrame == Time.frameCount))
			{
				return false;
			}

			// if we roll a dice and are below our chance rate, we're not allowed to play
			if (!EvaluateChance())
			{
				return false;
			}

			// if we are in cooldown, we're not allowed to play
			if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
					return false;
				}
			}

			// if all MMFeedbacks are disabled globally, we're not allowed to play
			if (!GlobalMMFeedbacksActive)
			{
				return false;
			}

			// if the game object this player is on disabled, we're not allowed to play
			if (!this.gameObject.activeInHierarchy)
			{
				return false;
			}
			
			// if we're using range and are not within range, we're not allowed to play
			if (OnlyPlayIfWithinRange)
			{
				if (RangeCenter == null)
				{
					return false;
				}
				float distanceToCenter = Vector3.Distance(position, RangeCenter.position);
				if (distanceToCenter > RangeDistance)
				{
					return false;
				}
			}

			return true;
		}
        
		protected virtual IEnumerator FrameOnePlayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			yield return null;
			this.enabled = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitForUnscaled(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}

		protected override void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
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
		
		protected override void CheckForPauses()
		{
			_pauseFound = false;
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					if ((FeedbacksList[i].Pause != null) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((FeedbacksList[i].HoldingPause == true) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
		}

		protected override void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			// if no pause was found, we just play all feedbacks at once
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbackCanPlay(FeedbacksList[i]))
				{
					FeedbacksList[i].Play(position, feedbacksIntensity);
				}
			}
		}

		protected override IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitForUnscaled(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected override void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
				PlayerCompleteFeedbacks();
				Events.TriggerOnComplete(this);
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > TotalDuration)
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
		/// A coroutine used to handle the sequence of feedbacks if pauses are involved
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected override IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			IsPlaying = true;

			int i = (Direction == Directions.TopToBottom) ? 0 : FeedbacksList.Count-1;

			int count = FeedbacksList.Count;
			while ((i >= 0) && (i < count))
			{
				if (!IsPlaying)
				{
					yield break;
				}

				if (FeedbacksList[i] == null)
				{
					yield break;
				}
                
				if (((FeedbacksList[i].Active) && (FeedbacksList[i].ScriptDrivenPause)) || InScriptDrivenPause)
				{
					InScriptDrivenPause = true;

					bool inAutoResume = (FeedbacksList[i].ScriptDrivenPauseAutoResume > 0f); 
					float scriptDrivenPauseStartedAt = GetTime();
					float autoResumeDuration = FeedbacksList[i].ScriptDrivenPauseAutoResume;
                    
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
				if ((FeedbacksList[i].Active)
				    && ((FeedbacksList[i].HoldingPause == true) || (FeedbacksList[i].LooperPause == true))
				    && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
				{
					Events.TriggerOnPause(this);
					// we stay here until all previous feedbacks have finished
					while ((GetTime() - _lastStartAt < _holdingMax) && !SkippingToTheEnd)
					{
						yield return null;
					}
					_holdingMax = 0f;
					_lastStartAt = GetTime();
				}

				// plays the feedback
				if (FeedbackCanPlay(FeedbacksList[i]))
				{
					FeedbacksList[i].Play(position, feedbacksIntensity);
				}

				// Handles pause
				if ((FeedbacksList[i].Pause != null) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection) && !SkippingToTheEnd)
				{
					bool shouldPause = true;
					if (FeedbacksList[i].Chance < 100)
					{
						float random = Random.Range(0f, 100f);
						if (random > FeedbacksList[i].Chance)
						{
							shouldPause = false;
						}
					}

					if (shouldPause)
					{
						yield return FeedbacksList[i].Pause;
						Events.TriggerOnResume(this);
						_lastStartAt = GetTime();
						_holdingMax = 0f;
					}
				}

				// updates holding max
				if (FeedbacksList[i].Active)
				{
					if ((FeedbacksList[i].Pause == null) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection) && (!FeedbacksList[i].Timing.ExcludeFromHoldingPauses))
					{
						float feedbackDuration = FeedbacksList[i].TotalDuration;
						_holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
					}
				}

				// handles looper
				if ((FeedbacksList[i].LooperPause == true)
				    && (FeedbacksList[i].Active)
				    && (FeedbacksList[i].ShouldPlayInThisSequenceDirection)
				    && (((FeedbacksList[i] as MMF_Looper).NumberOfLoopsLeft > 0) || (FeedbacksList[i] as MMF_Looper).InInfiniteLoop))
				{
					while (HasFeedbackStillPlaying() && !SkippingToTheEnd)
					{
						yield return null;
					}
	                
					// we determine the index we should start again at
					bool loopAtLastPause = (FeedbacksList[i] as MMF_Looper).LoopAtLastPause;
					bool loopAtLastLoopStart = (FeedbacksList[i] as MMF_Looper).LoopAtLastLoopStart;
                    
					int newi = 0;

					int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

					int listCount = FeedbacksList.Count;
					while ((j >= 0) && (j <= listCount))
					{
						// if we're at the start
						if (j == 0)
						{
							newi = j - 1;
							break;
						}
						if (j == listCount)
						{
							newi = j ;
							break;
						}
						// if we've found a pause
						if ((FeedbacksList[j].Pause != null)
						    && !SkippingToTheEnd
						    && (FeedbacksList[j].FeedbackDuration > 0f)
						    && loopAtLastPause && (FeedbacksList[j].Active))
						{
							newi = j;
							break;
						}
						// if we've found a looper start
						if ((FeedbacksList[j].LooperStart == true)
						    && !SkippingToTheEnd
						    && loopAtLastLoopStart
						    && (FeedbacksList[j].Active))
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
			while ((GetTime() - unscaledTimeAtEnd < _holdingMax) && !SkippingToTheEnd)
			{
				yield return null;
			}
			while (HasFeedbackStillPlaying() && !SkippingToTheEnd)
			{
				yield return null;
			}
			IsPlaying = false;
			PlayerCompleteFeedbacks();
			Events.TriggerOnComplete(this);
			ApplyAutoRevert();
		}

		protected virtual IEnumerator SkipToTheEndCo()
		{
			if (_startTime == GetTime())
			{
				yield return null;
			}
			SkippingToTheEnd = true;
			Events.TriggerOnSkipToTheEnd(this);
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].SkipToTheEnd(this.transform.position);    
				}
			}
			yield return null;
			yield return null;
			SkippingToTheEnd = false;
			StopFeedbacks();
		}

		#endregion

		#region STOP

		/// <summary>
		/// Stops all further feedbacks from playing, as well as stopping individual feedbacks 
		/// </summary>
		public override void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

		/// <summary>
		/// Stops all feedbacks from playing, with an option to also stop individual feedbacks
		/// </summary>
		public override void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

		/// <summary>
		/// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public override void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				int count = FeedbacksList.Count;
				for (int i = 0; i < count; i++)
				{
					FeedbacksList[i].Stop(position, feedbacksIntensity);
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}
        
		#endregion 

		#region CONTROLS

		/// <summary>
		/// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer. It's usually called automatically before playing them.
		/// </summary>
		public override void ResetFeedbacks()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

		/// <summary>
		/// Changes the direction of this MMFeedbacks
		/// </summary>
		public override void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

		/// <summary>
		/// Sets the direction of the player to the one specified in parameters
		/// </summary>
		public virtual void SetDirection(Directions newDirection)
		{
			Direction = newDirection;
		}
		
		/// <summary>
		/// Sets the direction to top to bottom
		/// </summary>
		public void SetDirectionTopToBottom()
		{
			Direction = Directions.TopToBottom;
		}

		/// <summary>
		/// Sets the direction to bottom to top
		/// </summary>
		public void SetDirectionBottomToTop()
		{
			Direction = Directions.BottomToTop;
		}
		
		/// <summary>
		/// When the player is done playing, we call PlayerComplete on all its feedbacks to let them know
		/// the player is done
		/// </summary>
		public virtual void PlayerCompleteFeedbacks()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].PlayerComplete();    
				}
			}
		}

		/// <summary>
		/// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
		/// </summary>
		public override void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

		/// <summary>
		/// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
		/// Note that this doesn't stop feedbacks, by design, but in most cases you'll probably want to call StopFeedbacks() first
		/// </summary>
		public virtual void RestoreInitialValues()
		{
			if (PlayCount <= 0)
			{
				return;
			}
			
			int count = FeedbacksList.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].RestoreInitialValues();    
				}
			}

			Events.TriggerOnRestoreInitialValues(this);
		}

		/// <summary>
		/// Forces initial vales on all feedbacks that support it.
		/// For example, a position feedback that'd move a Transform from A to B would move that transform to A
		/// </summary>
		public virtual void ForceInitialValues()
		{
			int count = FeedbacksList.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].ForceInitialValue(this.transform.position, FeedbacksIntensity);    
				}
			}
		}

		/// <summary>
		/// Skips to the end of a sequence of feedbacks. Note that depending on your setup, this can take up to 3 frames to complete, don't disable your player instantly, or it won't complete the skipping
		/// </summary>
		public virtual void SkipToTheEnd()
		{
			StartCoroutine(SkipToTheEndCo());
		}

		/// <summary>
		/// Resumes execution of a sequence if a script driven pause is in progress
		/// </summary>
		public override void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

		#endregion
        
		#region MODIFICATION

		/// <summary>
		/// Adds the specified MMF_Feedback to the player
		/// </summary>
		/// <param name="newFeedback"></param>
		public virtual void AddFeedback(MMF_Feedback newFeedback)
		{
			InitializeFeedbackList();
			newFeedback.Owner = this;
			newFeedback.UniqueID = Guid.NewGuid().GetHashCode();
			FeedbacksList.Add(newFeedback);
			newFeedback.OnAddFeedback();
			newFeedback.CacheRequiresSetup();
			newFeedback.InitializeCustomAttributes();
		}
        
		/// <summary>
		/// Adds a feedback of the specified type to the player
		/// </summary>
		/// <param name="feedbackType"></param>
		/// <returns></returns>
		public new MMF_Feedback AddFeedback(System.Type feedbackType, bool add = true)
		{
			InitializeFeedbackList();
			MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);
			newFeedback.Owner = this;
			newFeedback.Timing = new MMFeedbackTiming();
			newFeedback.UniqueID = Guid.NewGuid().GetHashCode();
			if (add)
			{
				FeedbacksList.Add(newFeedback);	
			}
			newFeedback.OnAddFeedback();
			newFeedback.InitializeCustomAttributes();
			newFeedback.CacheRequiresSetup();
			return newFeedback;
		}
        
		/// <summary>
		/// Removes the feedback at the specified index
		/// </summary>
		/// <param name="id"></param>
		public override void RemoveFeedback(int id)
		{
			if (FeedbacksList.Count < id)
			{
				return;
			}
			FeedbacksList.RemoveAt(id);
		}
        
		#endregion MODIFICATION

		#region HELPERS
        
		/// <summary>
		/// Returns true if feedbacks are still playing
		/// </summary>
		/// <returns></returns>
		public override bool HasFeedbackStillPlaying()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i].IsPlaying 
				     && !FeedbacksList[i].Timing.ExcludeFromHoldingPauses)
				    || FeedbacksList[i].Timing.RepeatForever)
				{
					return true;
				}
			}
			return false;
		}
        
		/// <summary>
		/// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
		/// </summary>
		protected override void CheckForLoops()
		{
			ContainsLoop = false;
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					if (FeedbacksList[i].LooperPause && FeedbacksList[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}

		/// <summary>
		/// Computes new random duration multipliers on all feedbacks if needed
		/// </summary>
		protected virtual void ComputeNewRandomDurationMultipliers()
		{
			if (RandomizeDuration)
			{
				_randomDurationMultiplier = Random.Range(RandomDurationMultiplier.x, RandomDurationMultiplier.y);
			}
			
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].RandomizeDuration))
				{
					FeedbacksList[i].ComputeNewRandomDurationMultiplier();
				}                
			}
		}

		/// <summary>
		/// Determines the intensity multiplier to apply 
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public virtual float ComputeRangeIntensityMultiplier(Vector3 position)
		{
			if (!OnlyPlayIfWithinRange)
			{
				return 1f;
			}

			if (RangeCenter == null)
			{
				return 0f;
			}

			float distanceToCenter = Vector3.Distance(position, RangeCenter.position);

			if (distanceToCenter > RangeDistance)
			{
				return 0f;
			}

			if (!UseRangeFalloff)
			{
				return 1f;
			}

			float normalizedDistance = MMFeedbacksHelpers.Remap(distanceToCenter, 0f, RangeDistance, 0f, 1f);
			float curveValue = RangeFalloff.Evaluate(normalizedDistance);
			float newIntensity = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, RemapRangeFalloff.x, RemapRangeFalloff.y);
			return newIntensity;
		}
        
		/// <summary>
		/// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		protected bool FeedbackCanPlay(MMF_Feedback feedback)
		{
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
		protected override void ApplyAutoRevert()
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
		public override float ApplyTimeMultiplier(float duration)
		{
			return duration * Mathf.Clamp(DurationMultiplier, _smallValue, float.MaxValue) * _randomDurationMultiplier;
		}

		/// <summary>
		/// Lets you destroy objects from feedbacks
		/// </summary>
		/// <param name="gameObjectToDestroy"></param>
		public virtual void ProxyDestroy(GameObject gameObjectToDestroy)
		{
			Destroy(gameObjectToDestroy);
		}
        
		/// <summary>
		/// Lets you destroy objects after a delay from feedbacks
		/// </summary>
		/// <param name="gameObjectToDestroy"></param>
		/// <param name="delay"></param>
		public virtual void ProxyDestroy(GameObject gameObjectToDestroy, float delay)
		{
			Destroy(gameObjectToDestroy, delay);
		}

		/// <summary>
		/// Lets you DestroyImmediate objects from feedbacks
		/// </summary>
		/// <param name="gameObjectToDestroy"></param>
		public virtual void ProxyDestroyImmediate(GameObject gameObjectToDestroy)
		{
			DestroyImmediate(gameObjectToDestroy);
		}
        
		#endregion

		#region ACCESS
		
		public enum AccessMethods { First, Previous, Closest, Next, Last }
		
		/// <summary>
		/// Returns the first feedback found in this player's list based on the chosen method and type
		/// First : first feedback of the matching type in the list, from top to bottom
		/// Previous : first feedback of the matching type located before (so above) the feedback at the reference index
		/// Closest : first feedback of the matching type located before or after the feedback at the reference index
		/// Next : first feedback of the matching type located after (so below) the feedback at the reference index
		/// First : last feedback of the matching type in the list, from top to bottom
		/// </summary>
		/// <param name="method"></param>
		/// <param name="referenceIndex"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetFeedbackOfType<T>(AccessMethods method, int referenceIndex) where T:MMF_Feedback
		{
			_t = typeof(T);

			referenceIndex = Mathf.Clamp(referenceIndex, 0, FeedbacksList.Count);
			
			switch (method)
			{
				case AccessMethods.First:
					for (int i = 0; i < FeedbacksList.Count; i++)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
				case AccessMethods.Previous:
					for (int i = referenceIndex; i >= 0; i--)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
				case AccessMethods.Closest:
					int closestIndexBack = referenceIndex;
					int closestIndexForward = referenceIndex;
					for (int i = referenceIndex; i >= 0; i--)
					{
						if (Check(i))
						{
							closestIndexBack = i;
							break;
						}
					}

					for (int i = referenceIndex; i < FeedbacksList.Count; i++)
					{
						if (Check(i))
						{
							closestIndexForward = i;
							break;
						}
					}

					int foundIndex;
					if ((closestIndexBack != referenceIndex) || (closestIndexForward != referenceIndex))
					{
						if (closestIndexBack == referenceIndex) { foundIndex = closestIndexForward; }
						else if (closestIndexForward == referenceIndex) { foundIndex = closestIndexBack; }
						else
						{
							int distanceBack = Mathf.Abs(referenceIndex - closestIndexBack);
							int distanceForward = Mathf.Abs(referenceIndex - closestIndexForward);
							foundIndex = (distanceBack > distanceForward) ? closestIndexForward : closestIndexBack;
						}
						return (T)FeedbacksList[foundIndex];
					}
					else
					{
						return null;
					}
				case AccessMethods.Next:
					for (int i = referenceIndex; i < FeedbacksList.Count; i++)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
				case AccessMethods.Last:
					for (int i = FeedbacksList.Count - 1; i >= 0; i--)
					{
						if (Check(i)) { return (T)FeedbacksList[i]; }
					}
					break;
			}
			return null;

			bool Check(int i)
			{
				return (FeedbacksList[i].GetType() == _t);
			}
		}

		/// <summary>
		/// Returns the first feedback of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetFeedbackOfType<T>() where T:MMF_Feedback
		{
			_t = typeof(T);
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if (feedback.GetType() == _t)
				{
					return (T)feedback;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns a list of all the feedbacks of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual List<T> GetFeedbacksOfType<T>() where T:MMF_Feedback
		{
			_t = typeof(T);
			List<T> list = new List<T>();
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if (feedback.GetType() == _t)
				{
					list.Add((T)feedback);
				}
			}
			return list;
		}

		/// <summary>
		/// Returns the first feedback of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetFeedbackOfType<T>(string searchedLabel) where T:MMF_Feedback
		{
			_t = typeof(T);
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if ((feedback.GetType() == _t) && (feedback.Label == searchedLabel))
				{
					return (T)feedback;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns a list of all the feedbacks of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual List<T> GetFeedbacksOfType<T>(string searchedLabel) where T:MMF_Feedback
		{
			_t = typeof(T);
			List<T> list = new List<T>();
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if ((feedback.GetType() == _t) && (feedback.Label == searchedLabel))
				{
					list.Add((T)feedback);
				}
			}
			return list;
		}

		#endregion
        
		#region EVENTS

		/// <summary>
		/// When we get a MMSetFeedbackRangeCenterEvent, we set our new range center
		/// </summary>
		/// <param name="newTransform"></param>
		protected virtual void OnMMSetFeedbackRangeCenterEvent(Transform newTransform)
		{
			if (IgnoreRangeEvents)
			{
				return;
			}
			RangeCenter = newTransform;
		}

		/// <summary>
		/// On Disable we stop all feedbacks
		/// </summary>
		protected override void OnDisable()
		{
			if (OnlyPlayIfWithinRange)
			{
				MMSetFeedbackRangeCenterEvent.Unregister(OnMMSetFeedbackRangeCenterEvent);	
			}
			
			if (IsPlaying)
			{
				if (StopFeedbacksOnDisable)
				{
					StopFeedbacks();    
				}
				StopAllCoroutines();
				for (int i = FeedbacksList.Count - 1; i >= 0; i--)
				{
					FeedbacksList[i].OnDisable();
				}
			}
		}

		/// <summary>
		/// On validate, we make sure our DurationMultiplier remains positive
		/// </summary>
		protected override void OnValidate()
		{
			RefreshCache();

			if ((FeedbacksList != null) && (FeedbacksList.Count > 0))
			{
				for (int i = FeedbacksList.Count - 1; i >= 0; i--)
				{
					FeedbacksList[i].OnValidate();	
				}	
			}
		}

		/// <summary>
		/// Refreshes cached feedbacks
		/// </summary>
		public virtual void RefreshCache()
		{
			if (FeedbacksList == null)
			{
				return;
			}
            
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
            
			for (int i = FeedbacksList.Count - 1; i >= 0; i--)
			{
				if (FeedbacksList[i] == null)
				{
					FeedbacksList.RemoveAt(i);
				}
				else
				{
					FeedbacksList[i].Owner = this;
					FeedbacksList[i].CacheRequiresSetup();
				}
			}

			ComputeCachedTotalDuration();
		}

		/// <summary>
		/// Computes the total duration of the player's sequence of feedbacks
		/// </summary>
		public virtual void ComputeCachedTotalDuration()
		{
			float total = 0f;
			if (FeedbacksList == null)
			{
				_cachedTotalDuration = InitialDelay;
				return;
			}
			
			CheckForPauses();

			if (!_pauseFound)
			{
				foreach (MMF_Feedback feedback in FeedbacksList)
				{
					feedback.ComputeTotalDuration();
					if ((feedback != null) && (feedback.Active) && feedback.ShouldPlayInThisSequenceDirection)
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
			}
			else
			{
				int lastLooperStart = 0;
				int lastLoopFoundAt = 0;
				int lastPauseFoundAt = 0;
				int loopsLeft = 0;
				int iterations = 0;
				int maxIterationsSafety = 1000;
				float currentPauseDelay = 0f;
				int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count-1;
				float intermediateTotal = 0f;
				while ((i >= 0) && (i < FeedbacksList.Count) && (iterations < maxIterationsSafety))
				{
					iterations++;
					
					if ((FeedbacksList[i] != null) && FeedbacksList[i].Active && FeedbacksList[i].ShouldPlayInThisSequenceDirection)
					{
						FeedbacksList[i].ComputeTotalDuration();
						if (FeedbacksList[i].Pause != null)
						{
							// pause
							if (FeedbacksList[i].HoldingPause)
							{
								intermediateTotal += (FeedbacksList[i] as MMF_Pause).PauseDuration;
								total += intermediateTotal;
								intermediateTotal = 0f;
							}
							else
							{
								currentPauseDelay += (FeedbacksList[i] as MMF_Pause).PauseDuration;
							}
							
							//loops
							if (FeedbacksList[i].LooperStart)
							{
								lastLooperStart = i;
							}

							if (!FeedbacksList[i].LooperPause)
							{
								lastPauseFoundAt = i;
							}

							if (FeedbacksList[i].LooperPause && ((FeedbacksList[i] as MMF_Looper).NumberOfLoops > 0))
							{
								if (i == lastLoopFoundAt)
								{
									loopsLeft--;
									if (loopsLeft <= 0)
									{
										i += (Direction == Directions.TopToBottom) ? 1 : -1;
										continue;
									}
								}
								else
								{
									lastLoopFoundAt = i;
									loopsLeft = (FeedbacksList[i] as MMF_Looper).NumberOfLoops - 1;
								}
								
								if ((FeedbacksList[i] as MMF_Looper).InfiniteLoop)
								{
									_cachedTotalDuration = 999f; 
									return;
								}

								if ((FeedbacksList[i] as MMF_Looper).LoopAtLastPause)
								{
									i = lastPauseFoundAt;
									total += intermediateTotal;
									intermediateTotal = 0f;
									currentPauseDelay = 0f;
									continue;
								}
								else if ((FeedbacksList[i] as MMF_Looper).LoopAtLastLoopStart)
								{
									i = lastLooperStart;
									total += intermediateTotal;
									intermediateTotal = 0f;
									currentPauseDelay = 0f;
									continue;
								}
								else
								{
									i = 0;
									total += intermediateTotal;
									intermediateTotal = 0f;
									currentPauseDelay = 0f;
									continue;
								}
							}	
						}
						else
						{
							float feedbackDuration = FeedbacksList[i].TotalDuration + currentPauseDelay;
							if (intermediateTotal < feedbackDuration)
							{
								intermediateTotal = feedbackDuration;    
							}
						}
					}
					
					i += (Direction == Directions.TopToBottom) ? 1 : -1;
				}
				total += intermediateTotal;
			}
			_cachedTotalDuration = InitialDelay + total;
		}

		/// <summary>
		/// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
		/// </summary>
		protected override void OnDestroy()
		{
			IsPlaying = false;
            
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				feedback.OnDestroy();
			}
		}

		/// <summary>
		/// Draws gizmos, when the MMF_Player is selected, for all feedbacks that implement the method of the same name 
		/// </summary>
		protected void OnDrawGizmosSelected()
		{
			if (FeedbacksList == null)
			{
				return;
			}
            
			for (int i = FeedbacksList.Count - 1; i >= 0; i--)
			{
				FeedbacksList[i].OnDrawGizmosSelectedHandler();
			}
		}

		#endregion EVENTS
	}    
}