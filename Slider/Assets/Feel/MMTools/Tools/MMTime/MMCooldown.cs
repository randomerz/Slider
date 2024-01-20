using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class to handle cooldown related properties and their resource consumption over time
	/// Remember to initialize it (once) and update it every frame from another class
	/// </summary>
	[System.Serializable]    
	public class MMCooldown 
	{
		/// all possible states for the object
		public enum CooldownStates { Idle, Consuming, Stopped, Refilling }
		/// if this is true, the cooldown won't do anything
		public bool Unlimited = false;
		/// the time it takes, in seconds, to consume the object
		public float ConsumptionDuration = 2f;
		/// the pause to apply before refilling once the object's been depleted
		public float PauseOnEmptyDuration = 1f;
		/// the duration of the refill, in seconds, if uninterrupted
		public float RefillDuration = 1f;
		/// whether or not the refill can be interrupted by a new Start instruction
		public bool CanInterruptRefill = true;
		[MMReadOnly]
		/// the current state of the object
		public CooldownStates CooldownState = CooldownStates.Idle;
		[MMReadOnly]
		/// the amount of duration left in the object at any given time
		public float CurrentDurationLeft;
		
		/// <summary>
		/// A public delegate you can listen to for state changes
		///
		/// How to use : 
		///
		/// private void OnCooldownStateChange(MMCooldown.CooldownStates newState)
		/// {
		///		if (newState == MMCooldown.CooldownStates.Stopped)
		///		{
		/// 		// do something
		///		}
		/// }
		///
		/// private void OnEnable()	{ Cooldown.OnStateChange += OnCooldownStateChange; }		
		/// private void OnDisable() { Cooldown.OnStateChange -= OnCooldownStateChange;	}
		///
		/// </summary>
		public delegate void OnStateChangeDelegate(CooldownStates newState);
		public OnStateChangeDelegate OnStateChange;

		protected float _emptyReachedTimestamp = 0f;

		/// <summary>
		/// An init method that ensures the object is reset
		/// </summary>
		public virtual void Initialization()
		{
			CurrentDurationLeft = ConsumptionDuration;
			ChangeState(CooldownStates.Idle);
			_emptyReachedTimestamp = 0f;
		}

		/// <summary>
		/// Starts consuming the cooldown object if possible
		/// </summary>
		public virtual void Start()
		{
			if (Ready())
			{
				ChangeState(CooldownStates.Consuming);
			}
		}

		/// <summary>
		/// Returns true if the cooldown is ready to be consumed, false otherwise
		/// </summary>
		/// <returns></returns>
		public virtual bool Ready()
		{
			if (Unlimited)
			{
				return true;
			}
			if (CooldownState == CooldownStates.Idle)
			{
				return true;
			}
			if ((CooldownState == CooldownStates.Refilling) && (CanInterruptRefill))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Stops consuming the object 
		/// </summary>
		public virtual void Stop()
		{
			if (CooldownState == CooldownStates.Consuming)
			{
				ChangeState(CooldownStates.Stopped);
			}
		}
        
		public float Progress 
		{
			get
			{
				if (Unlimited)
				{
					return 1f;
				}
                
				if (CooldownState == CooldownStates.Consuming || CooldownState == CooldownStates.Stopped)
				{
					return 0f;
				}

				if (CooldownState == CooldownStates.Refilling)
				{
					return CurrentDurationLeft / RefillDuration;
				}
                
				return 1f;
			}
		}

		/// <summary>
		/// Processes the object's state machine
		/// </summary>
		public virtual void Update()
		{
			if (Unlimited)
			{
				return;
			}

			switch (CooldownState)
			{
				case CooldownStates.Idle:
					break;

				case CooldownStates.Consuming:
					CurrentDurationLeft = CurrentDurationLeft - Time.deltaTime;
					if (CurrentDurationLeft <= 0f)
					{
						CurrentDurationLeft = 0f;
						_emptyReachedTimestamp = Time.time;
						ChangeState(CooldownStates.Stopped);
					}
					break;

				case CooldownStates.Stopped:
					if (Time.time - _emptyReachedTimestamp >= PauseOnEmptyDuration)
					{
						ChangeState(CooldownStates.Refilling);
					}
					break;

				case CooldownStates.Refilling:
					CurrentDurationLeft += (RefillDuration * Time.deltaTime) / RefillDuration;
					if (CurrentDurationLeft >= RefillDuration)
					{
						CurrentDurationLeft = ConsumptionDuration;
						ChangeState(CooldownStates.Idle);
					}
					break;
			}
		}

		/// <summary>
		/// Changes the current state of the cooldown and invokes the delegate if needed
		/// </summary>
		/// <param name="newState"></param>
		protected virtual void ChangeState(CooldownStates newState)
		{
			CooldownState = newState;
			OnStateChange?.Invoke(newState);
		}
	}
}