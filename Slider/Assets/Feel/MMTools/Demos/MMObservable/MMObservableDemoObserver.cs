using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A test class used to demonstrate the MMObservable in the MMObservableTest demo scene    
	/// </summary>
	public class MMObservableDemoObserver : MonoBehaviour
	{
		/// the subject to look at
		public MMObservableDemoSubject TargetSubject;    

		/// <summary>
		/// When the position changes, we move our object accordingly on the y axis
		/// </summary>
		protected virtual void OnPositionChange()
		{
			this.transform.position = this.transform.position.MMSetY(TargetSubject.PositionX.Value);
		}
        
		/// <summary>
		/// On enable we start listening for changes
		/// </summary>
		protected virtual void OnEnable()
		{
			TargetSubject.PositionX.OnValueChanged += OnPositionChange;
		}

		/// <summary>
		/// On enable we stop listening for changes
		/// </summary>
		protected virtual void OnDisable()
		{
			TargetSubject.PositionX.OnValueChanged -= OnPositionChange;
		}
	}
}