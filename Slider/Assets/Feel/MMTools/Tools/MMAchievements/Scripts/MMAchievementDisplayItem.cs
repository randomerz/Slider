using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class is used to display an achievement. Add it to a prefab containing all the required elements listed below.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Achievements/MMAchievementDisplayItem")]
	public class MMAchievementDisplayItem : MonoBehaviour 
	{		
		public Image BackgroundLocked;
		public Image BackgroundUnlocked;
		public Image Icon;
		public Text Title;
		public Text Description;
		public MMProgressBar ProgressBarDisplay;	
	}
}