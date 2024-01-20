using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This very simple class simply exposes a method to toggle the GameObject it's on (or a target one if left empty in the inspector) active or inactive
	/// </summary>
	public class MMToggleActive : MonoBehaviour
	{
		[Header("Target - leave empty for self")]
		/// the target gameobject to toggle. Leave blank for auto grab
		public GameObject TargetGameObject;

		/// a test button
		[MMInspectorButton("ToggleActive")]        
		public bool ToggleActiveButton;

		/// <summary>
		/// On awake, grabs self if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (TargetGameObject == null)
			{
				TargetGameObject = this.gameObject;
			}
		}

		/// <summary>
		/// Toggles the target gameobject's active state
		/// </summary>
		public virtual void ToggleActive()
		{
			TargetGameObject.SetActive(!TargetGameObject.activeInHierarchy);
		}
	}
}