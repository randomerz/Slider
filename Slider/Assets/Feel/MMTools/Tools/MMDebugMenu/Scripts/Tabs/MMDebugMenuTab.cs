using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to handle the display of a tab in a MMDebugMenu
	/// </summary>
	public class MMDebugMenuTab : MonoBehaviour
	{
		/// the tab's title
		public Text TabText;
		/// the tab's background image
		public Image TabBackground;
		/// the color to use for the background when the tab is selected
		public Color SelectedBackgroundColor;
		/// the color to use for the background when the tab is not selected
		public Color DeselectedBackgroundColor;
		/// the color to use for the text when the tab is selected
		public Color SelectedTextColor;
		/// the color to use for the text when the tab is not selected
		public Color DeselectedTextColor;
		/// the index of that tab, auto setup by the manager
		public int Index;
		/// the manager for this tab, auto setup
		public MMDebugMenuTabManager Manager;
		/// if this is true, scale will be forced to one on init
		public bool ForceScaleOne = true;

		/// <summary>
		/// On Start we initialize this tab item
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

		/// <summary>
		/// Selects this tab
		/// </summary>
		public virtual void Select()
		{
			Manager.Select(Index);
			TabText.color = SelectedTextColor;
			TabBackground.color = SelectedBackgroundColor;
		}

		/// <summary>
		/// Deselects this tab
		/// </summary>
		public virtual void Deselect()
		{
			TabText.color = DeselectedTextColor;
			TabBackground.color = DeselectedBackgroundColor;
		}
	}
}