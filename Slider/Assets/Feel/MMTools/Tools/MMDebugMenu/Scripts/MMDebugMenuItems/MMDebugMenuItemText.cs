using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to bind a text item to a MMDebugMenu
	/// </summary>
	public class MMDebugMenuItemText : MonoBehaviour
	{
		[Header("Bindings")]
		/// a text comp used to display the text
		[TextArea]
		public Text ContentText;
	}
}