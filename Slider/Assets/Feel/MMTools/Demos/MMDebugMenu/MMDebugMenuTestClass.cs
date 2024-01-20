using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple test class used in the MMDebugMenu demo scene to shake a few values and output them in the debug on screen console
	/// </summary>
	public class MMDebugMenuTestClass : MonoBehaviour
	{
		/// a label to display
		public string Label;

		private float multiplier;

		/// <summary>
		/// On starts, randomizes a multiplier
		/// </summary>
		private void Start()
		{
			multiplier = Random.Range(0f, 50000f);
		}
		/// <summary>
		/// On update, outputs a text on screen
		/// </summary>
		void Update()
		{
			float test = (Mathf.Sin(Time.time) + 2) * multiplier;
			MMDebug.DebugOnScreen(Label, test);
		}
	}
}