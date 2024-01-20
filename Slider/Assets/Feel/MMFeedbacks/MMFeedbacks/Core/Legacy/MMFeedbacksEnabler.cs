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
	public class MMFeedbacksEnabler : MonoBehaviour
	{
		/// the MMFeedbacks to pilot
		public MMFeedbacks TargetMMFeedbacks { get; set; }
        
		/// <summary>
		/// On enable, we re-enable (and thus play) our MMFeedbacks if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if ((TargetMMFeedbacks != null) && !TargetMMFeedbacks.enabled && TargetMMFeedbacks.AutoPlayOnEnable)
			{
				TargetMMFeedbacks.enabled = true;
			}
		}
	}    
}