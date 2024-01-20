using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A test class used to demonstrate how MMObservable works in the MMObservableTest demo scene  
	/// </summary>
	public class MMObservableDemoSubject : MonoBehaviour
	{
		/// a public float we expose, outputting the x position of our object
		public MMObservable<float> PositionX = new MMObservable<float>();

		/// <summary>
		/// On Update we update our x position
		/// </summary>
		protected virtual void Update()
		{
			PositionX.Value = this.transform.position.x;
		}
	}
}