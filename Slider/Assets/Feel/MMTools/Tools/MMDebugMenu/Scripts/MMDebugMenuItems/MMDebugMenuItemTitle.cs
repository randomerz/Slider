using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to bind a title item to a MMDebugMenu
	/// </summary>
	public class MMDebugMenuItemTitle : MonoBehaviour
	{
		[Header("Bindings")]
		/// the text comp used to display the title
		public Text TitleText;
		/// a line below the title
		public Image TitleLine;
	}
}