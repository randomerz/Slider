using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A data class to store auto execution info to be used in MMAutoExecution
	/// </summary>
	[System.Serializable]
	public class MMAutoExecutionItem
	{
		/// if this is true, Event will be invoked on Awake  
		public bool AutoExecuteOnAwake;
		/// if this is true, Event will be invoked on Enable
		public bool AutoExecuteOnEnable;
		/// if this is true, Event will be invoked on Disable
		public bool AutoExecuteOnDisable;
		/// if this is true, Event will be invoked on Start
		public bool AutoExecuteOnStart;
		/// if this is true, Event will be invoked on Instantiate (you'll need to send a OnInstantiate message for this to happen
		public bool AutoExecuteOnInstantiate;
		public UnityEvent Event;
	}
    
	/// <summary>
	/// This simple class lets you trigger Unity events automatically, on Awake, Enable, Disable, Start, or on instantiate
	/// For that last one, you'll want to send a "OnInstantiate" message when instantiating this object
	/// </summary>
	public class MMAutoExecution : MonoBehaviour
	{
		/// a list of events to trigger automatically
		public List<MMAutoExecutionItem> Events;

		/// <summary>
		/// On Awake we invoke our events if needed
		/// </summary>
		protected virtual void Awake()
		{
			foreach (MMAutoExecutionItem item in Events)
			{
				if ((item.AutoExecuteOnAwake) && (item.Event != null))
				{
					item.Event.Invoke();
				}
			}
		}
        
		/// <summary>
		/// On Start we invoke our events if needed
		/// </summary>
		protected virtual void Start()
		{
			foreach (MMAutoExecutionItem item in Events)
			{
				if ((item.AutoExecuteOnStart) && (item.Event != null))
				{
					item.Event.Invoke();
				}
			}
		}
        
		/// <summary>
		/// On Enable we invoke our events if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			foreach (MMAutoExecutionItem item in Events)
			{
				if ((item.AutoExecuteOnEnable) && (item.Event != null))
				{
					item.Event.Invoke();
				}
			}
		}
        
		/// <summary>
		/// On Enable we invoke our events if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			foreach (MMAutoExecutionItem item in Events)
			{
				if ((item.AutoExecuteOnDisable) && (item.Event != null))
				{
					item.Event.Invoke();
				}
			}
		}
        
		/// <summary>
		/// On Instantiate we invoke our events if needed
		/// </summary>
		protected virtual void OnInstantiate()
		{
			foreach (MMAutoExecutionItem item in Events)
			{
				if ((item.AutoExecuteOnInstantiate) && (item.Event != null))
				{
					item.Event.Invoke();
				}
			}
		}
	}    
}