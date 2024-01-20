using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to keep track of tabs and their contents in a MMDebugMenu
	/// </summary>
	public class MMDebugMenuTabManager : MonoBehaviour
	{
		/// a list of all the tabs under that manager
		public List<MMDebugMenuTab> Tabs;
		/// a list of all the tabs contents under that manager
		public List<MMDebugMenuTabContents> TabsContents;

		/// <summary>
		/// Selects a tab, hides the others
		/// </summary>
		/// <param name="selected"></param>
		public virtual void Select(int selected)
		{
			foreach(MMDebugMenuTab tab in Tabs)
			{
				if (tab.Index != selected)
				{
					tab.Deselect();
				}
			}
			foreach(MMDebugMenuTabContents contents in TabsContents)
			{
				if (contents.Index == selected)
				{
					contents.gameObject.SetActive(true);
				}
				else
				{
					contents.gameObject.SetActive(false);
				}
			}
		}
	}
}