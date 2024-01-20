using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Actions are behaviours and describe what your character is doing. Examples include patrolling, shooting, jumping, etc. 
	/// </summary>
	public abstract class AIAction : MonoBehaviour
	{
		public enum InitializationModes { EveryTime, OnlyOnce, }

		public InitializationModes InitializationMode;
		protected bool _initialized;
		
		public string Label;
		public abstract void PerformAction();
		public bool ActionInProgress { get; set; }
		protected AIBrain _brain;

		protected virtual bool ShouldInitialize
		{
			get
			{
				switch (InitializationMode)
				{
					case InitializationModes.EveryTime:
						return true;
					case InitializationModes.OnlyOnce:
						return _initialized == false;
				}
				return true;
			}
		}

		/// <summary>
		/// On Awake we grab our AIBrain
		/// </summary>
		protected virtual void Awake()
		{
			_brain = this.gameObject.GetComponentInParent<AIBrain>();
		}

		/// <summary>
		/// Initializes the action. Meant to be overridden
		/// </summary>
		public virtual void Initialization()
		{
			_initialized = true;
		}

		/// <summary>
		/// Describes what happens when the brain enters the state this action is in. Meant to be overridden.
		/// </summary>
		public virtual void OnEnterState()
		{
			ActionInProgress = true;
		}

		/// <summary>
		/// Describes what happens when the brain exits the state this action is in. Meant to be overridden.
		/// </summary>
		public virtual void OnExitState()
		{
			ActionInProgress = false;
		}
	}
}