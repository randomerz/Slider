using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A helper class added automatically by MMFeedbacks if they're in AutoPlayOnEnable mode
	/// This lets them play again should their parent game object be disabled/enabled
	/// </summary>
	[AddComponentMenu("")]
	public class MMF_PlayerEnabler : MonoBehaviour
	{
		/// the MMFeedbacks to pilot
		public MMF_Player TargetMmfPlayer { get; set; }
        
		/// <summary>
		/// On enable, we re-enable (and thus play) our MMFeedbacks if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if ((TargetMmfPlayer != null) && !TargetMmfPlayer.enabled && TargetMmfPlayer.AutoPlayOnEnable)
			{
				TargetMmfPlayer.enabled = true;
			}
		}
	}    
}