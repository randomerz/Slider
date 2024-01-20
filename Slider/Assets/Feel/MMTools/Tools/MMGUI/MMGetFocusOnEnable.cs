using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this helper to an object and focus will be set to it on Enable
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/GUI/MMGetFocusOnEnable")]
	public class MMGetFocusOnEnable : MonoBehaviour
	{
		protected virtual void OnEnable()
		{
			EventSystem.current.SetSelectedGameObject(this.gameObject, null);
		}
	}
}