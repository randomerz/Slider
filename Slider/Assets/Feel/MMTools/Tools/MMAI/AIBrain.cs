using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
	/// <summary>
	/// the AI brain is responsible from going from one state to the other based on the defined transitions. It's basically just a collection of states, and it's where you'll link all the actions, decisions, states and transitions together.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/AI/AIBrain")]
	public class AIBrain : MonoBehaviour
	{
		[Header("Debug")]
		/// the owner of that AI Brain, usually the associated character
		[MMReadOnly]
		public GameObject Owner;
		/// the collection of states
		public List<AIState> States;
		/// this brain's current state
		public AIState CurrentState { get; protected set; }
		/// the time we've spent in the current state
		[MMReadOnly]
		public float TimeInThisState;
		/// the current target
		[MMReadOnly]
		public Transform Target;
		/// the last known world position of the target
		[MMReadOnly]
		public Vector3 _lastKnownTargetPosition = Vector3.zero;
		
		[Header("State")]
		/// whether or not this brain is active
		public bool BrainActive = true;
		public bool ResetBrainOnStart = true;
		public bool ResetBrainOnEnable = false;

		[Header("Frequencies")]
		/// the frequency (in seconds) at which to perform actions (lower values : higher frequency, high values : lower frequency but better performance)
		public float ActionsFrequency = 0f;
		/// the frequency (in seconds) at which to evaluate decisions
		public float DecisionFrequency = 0f;
        
		/// whether or not to randomize the action and decision frequencies
		public bool RandomizeFrequencies = false;
		/// the min and max values between which to randomize the action frequency
		[MMVector("min","max")]
		public Vector2 RandomActionFrequency = new Vector2(0.5f, 1f);
		/// the min and max values between which to randomize the decision frequency
		[MMVector("min","max")]
		public Vector2 RandomDecisionFrequency = new Vector2(0.5f, 1f);

		protected AIDecision[] _decisions;
		protected AIAction[] _actions;
		protected float _lastActionsUpdate = 0f;
		protected float _lastDecisionsUpdate = 0f;
		protected AIState _initialState;

		public virtual AIAction[] GetAttachedActions()
		{
			AIAction[] actions = this.gameObject.GetComponentsInChildren<AIAction>();
			return actions;
		}

		public virtual AIDecision[] GetAttachedDecisions()
		{
			AIDecision[] decisions = this.gameObject.GetComponentsInChildren<AIDecision>();
			return decisions;
		}

		protected virtual void OnEnable()
		{
			if (ResetBrainOnEnable)
			{
				ResetBrain();
			}
		}

		/// <summary>
		/// On awake we set our brain for all states
		/// </summary>
		protected virtual void Awake()
		{
			foreach (AIState state in States)
			{
				state.SetBrain(this);
			}
			_decisions = GetAttachedDecisions();
			_actions = GetAttachedActions();
			if (RandomizeFrequencies)
			{
				ActionsFrequency = Random.Range(RandomActionFrequency.x, RandomActionFrequency.y);
				DecisionFrequency = Random.Range(RandomDecisionFrequency.x, RandomDecisionFrequency.y);
			}
		}

		/// <summary>
		/// On Start we set our first state
		/// </summary>
		protected virtual void Start()
		{
			if (ResetBrainOnStart)
			{
				ResetBrain();	
			}
		}

		/// <summary>
		/// Every frame we update our current state
		/// </summary>
		protected virtual void Update()
		{
			if (!BrainActive || (CurrentState == null) || (Time.timeScale == 0f))
			{
				return;
			}

			if (Time.time - _lastActionsUpdate > ActionsFrequency)
			{
				CurrentState.PerformActions();
				_lastActionsUpdate = Time.time;
			}
            
			if (!BrainActive)
			{
				return;
			}
            
			if (Time.time - _lastDecisionsUpdate > DecisionFrequency)
			{
				CurrentState.EvaluateTransitions();
				_lastDecisionsUpdate = Time.time;
			}
            
			TimeInThisState += Time.deltaTime;

			StoreLastKnownPosition();
		}
        
		/// <summary>
		/// Transitions to the specified state, trigger exit and enter states events
		/// </summary>
		/// <param name="newStateName"></param>
		public virtual void TransitionToState(string newStateName)
		{
			if (CurrentState == null)
			{
				CurrentState = FindState(newStateName);
				if (CurrentState != null)
				{
					CurrentState.EnterState();
				}
				return;
			}
			if (newStateName != CurrentState.StateName)
			{
				CurrentState.ExitState();
				OnExitState();

				CurrentState = FindState(newStateName);
				if (CurrentState != null)
				{
					CurrentState.EnterState();
				}                
			}
		}
        
		/// <summary>
		/// When exiting a state we reset our time counter
		/// </summary>
		protected virtual void OnExitState()
		{
			TimeInThisState = 0f;
		}

		/// <summary>
		/// Initializes all decisions
		/// </summary>
		protected virtual void InitializeDecisions()
		{
			if (_decisions == null)
			{
				_decisions = GetAttachedDecisions();
			}
			foreach(AIDecision decision in _decisions)
			{
				decision.Initialization();
			}
		}

		/// <summary>
		/// Initializes all actions
		/// </summary>
		protected virtual void InitializeActions()
		{
			if (_actions == null)
			{
				_actions = GetAttachedActions();
			}
			foreach(AIAction action in _actions)
			{
				action.Initialization();
			}
		}

		/// <summary>
		/// Returns a state based on the specified state name
		/// </summary>
		/// <param name="stateName"></param>
		/// <returns></returns>
		protected AIState FindState(string stateName)
		{
			foreach (AIState state in States)
			{
				if (state.StateName == stateName)
				{
					return state;
				}
			}
			if (stateName != "")
			{
				Debug.LogError("You're trying to transition to state '" + stateName + "' in " + this.gameObject.name + "'s AI Brain, but no state of this name exists. Make sure your states are named properly, and that your transitions states match existing states.");
			}            
			return null;
		}

		/// <summary>
		/// Stores the last known position of the target
		/// </summary>
		protected virtual void StoreLastKnownPosition()
		{
			if (Target != null)
			{
				_lastKnownTargetPosition = Target.transform.position;
			}
		}

		/// <summary>
		/// Resets the brain, forcing it to enter its first state
		/// </summary>
		public virtual void ResetBrain()
		{
			InitializeDecisions();
			InitializeActions();
			BrainActive = true;
			this.enabled = true;

			if (CurrentState != null)
			{
				CurrentState.ExitState();
				OnExitState();
			}
            
			if (States.Count > 0)
			{
				CurrentState = States[0];
				CurrentState?.EnterState();
			}  
		}
	}
}