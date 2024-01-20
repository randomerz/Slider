using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to describe tab contents
	/// </summary>
	public class MMDebugMenuTabContents : MonoBehaviour
	{
		/// the index of the tab, setup by MMDebugMenu
		public int Index = 0;
		/// the parent of the tab, setup by MMDebugMenu
		public Transform Parent;
		/// if this is true, scale will be forced to one on init
		public bool ForceScaleOne = true;

		/// <summary>
		/// On Start we initialize this tab contents
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we force the scale to one
		/// </summary>
		protected virtual void Initialization()
		{
			if (ForceScaleOne)
			{
				this.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
			}            
		}
	}
}