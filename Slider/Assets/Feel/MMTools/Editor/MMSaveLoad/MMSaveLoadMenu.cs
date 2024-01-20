using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Adds a dedicated Tools menu into the top bar More Mountains entry to delete all saved data
	/// </summary>
	public static class MMSaveLoadMenu 
	{
		[MenuItem("Tools/More Mountains/Delete all saved data",false,31)]
		/// <summary>
		/// Adds a menu item to reset all data saved by the MMSaveLoadManager. No turning back.
		/// </summary>
		private static void ResetAllSavedInventories()
		{
			MMSaveLoadManager.DeleteAllSaveFiles();
			Debug.LogFormat ("All Save Files Deleted");
		}
	}
}