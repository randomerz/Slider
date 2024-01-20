using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public struct MMStateChangeEvent<T> where T: struct, IComparable, IConvertible, IFormattable
	{
		public GameObject Target;
		public MMStateMachine<T> TargetStateMachine;
		public T NewState;
		public T PreviousState;

		public MMStateChangeEvent(MMStateMachine<T> stateMachine)
		{
			Target = stateMachine.Target;
			TargetStateMachine = stateMachine;
			NewState = stateMachine.CurrentState;
			PreviousState = stateMachine.PreviousState;
		}
	}

	/// <summary>
	/// Public interface for the state machine.
	/// </summary>
	public interface MMIStateMachine
	{
		bool TriggerEvents { get; set; }
	}

	/// <summary>
	/// StateMachine manager, designed with simplicity in mind (as simple as a state machine can be anyway).
	/// To use it, you need an enum. For example : public enum CharacterConditions { Normal, ControlledMovement, Frozen, Paused, Dead }
	/// Declare it like so : public StateMachine<CharacterConditions> ConditionStateMachine;
	/// Initialize it like that : ConditionStateMachine = new StateMachine<CharacterConditions>();
	/// Then from anywhere, all you need to do is update its state when needed, like that for example : ConditionStateMachine.ChangeState(CharacterConditions.Dead);
	/// The state machine will store for you its current and previous state, accessible at all times, and will also optionnally trigger events on enter/exit of these states.
	/// </summary>
	public class MMStateMachine<T> : MMIStateMachine where T : struct, IComparable, IConvertible, IFormattable
	{
		/// If you set TriggerEvents to true, the state machine will trigger events when entering and exiting a state. 
		/// Additionnally, it has options to trigger events on state change that can be listened to from any listener, without a delegate's hard binding, like so :
		/// let's assume in some class we have a public MMStateMachine<CharacterStates.MovementStates> MovementState, and we use that to track the state of a moving character (idle, walking, running etc)
		/// in any other class, we could do :
		/// public class TestListener : MonoBehaviour, MMEventListener<MMStateChangeEvent<CharacterStates.MovementStates>>
		/// {
		/// 	// triggered every time a state change event occurs
		/// 	public void OnMMEvent(MMStateChangeEvent<CharacterStates.MovementStates> stateChangeEvent)
		/// 	{
		/// 		if (stateChangeEvent.NewState == CharacterStates.MovementStates.Crawling)
		/// 		{
		/// 			//do something - in a real life scenario you'd probably make sure you have the right target, etc.
		/// 		}
		/// 	}
		/// 
		/// 	private void OnEnable() // on enable we start listening for these events
		/// 	{
		/// 		MMEventManager.AddListener<MMStateChangeEvent<CharacterStates.MovementStates>>(this);
		/// 	}
		/// 
		/// 	private void OnDisable() // on disable we stop listening for these events
		/// 	{
		/// 		MMEventManager.RemoveListener<MMStateChangeEvent<CharacterStates.MovementStates>>(this);
		/// 	}
		/// }
		/// Now every time this character's movement state changes, the OnMMEvent method will be called, and you can do whatever you want with it.
		/// 
		/// whether or not this state machine broadcasts events 
		public bool TriggerEvents { get; set; }
		/// the name of the target gameobject
		public GameObject Target;
		/// the current character's movement state
		public T CurrentState { get; protected set; }
		/// the character's movement state before entering the current one
		public T PreviousState { get; protected set; }

		public delegate void OnStateChangeDelegate();
		/// an event you can listen to to listen locally to changes on that state machine
		/// to listen to them, from any class : 
		/// void OnEnable()
		/// {
		///    yourReferenceToTheStateMachine.OnStateChange += OnStateChange;
		/// }
		/// void OnDisable()
		/// {
		///    yourReferenceToTheStateMachine.OnStateChange -= OnStateChange;
		/// }
		/// void OnStateChange()
		/// {
		///    // Do something
		/// }
		public OnStateChangeDelegate OnStateChange;

		/// <summary>
		/// Creates a new StateMachine, with a targetName (used for events, usually use GetInstanceID()), and whether you want to use events with it or not
		/// </summary>
		/// <param name="targetName">Target name.</param>
		/// <param name="triggerEvents">If set to <c>true</c> trigger events.</param>
		public MMStateMachine(GameObject target, bool triggerEvents)
		{
			this.Target = target;
			this.TriggerEvents = triggerEvents;
		} 

		/// <summary>
		/// Changes the current movement state to the one specified in the parameters, and triggers exit and enter events if needed
		/// </summary>
		/// <param name="newState">New state.</param>
		public virtual void ChangeState(T newState)
		{
			// if the "new state" is the current one, we do nothing and exit
			if (EqualityComparer<T>.Default.Equals(newState, CurrentState))
			{
				return;
			}

			// we store our previous character movement state
			PreviousState = CurrentState;
			CurrentState = newState;

			OnStateChange?.Invoke();

			if (TriggerEvents)
			{
				MMEventManager.TriggerEvent (new MMStateChangeEvent<T> (this));
			}
		}

		/// <summary>
		/// Returns the character to the state it was in before its current state
		/// </summary>
		public virtual void RestorePreviousState()
		{
			// we restore our previous state
			CurrentState = PreviousState;

			OnStateChange?.Invoke();

			if (TriggerEvents)
			{
				MMEventManager.TriggerEvent (new MMStateChangeEvent<T> (this));
			}
		}	
	}
}