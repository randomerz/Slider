using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// An asset to store copy information, as well as global feedback settings.
	/// It requires that one (and only one) MMFeedbacksConfiguration asset be created and stored in a Resources folder.
	/// That's already done when installing MMFeedbacks.
	/// </summary>
	[CreateAssetMenu(menuName = "MoreMountains/MMFeedbacks/Configuration", fileName = "MMFeedbacksConfiguration")]
	public class MMFeedbacksConfiguration : ScriptableObject
	{
		private static MMFeedbacksConfiguration _instance;
		private static bool _instantiated;
        
		/// <summary>
		/// Singleton pattern
		/// </summary>
		public static MMFeedbacksConfiguration Instance
		{
			get
			{
				if (_instantiated)
				{
					return _instance;
				}
                
				string assetName = typeof(MMFeedbacksConfiguration).Name;
                
				MMFeedbacksConfiguration loadedAsset = Resources.Load<MMFeedbacksConfiguration>("MMFeedbacksConfiguration");
				_instantiated = true;
				_instance = loadedAsset;
                
				return _instance;
			}
		}

		[Header("Debug")]
		/// storage for copy/paste
		public MMFeedbacks _mmFeedbacks;
        
		[Header("Help settings")]
		/// if this is true, inspector tips will be shown for MMFeedbacks
		public bool ShowInspectorTips = true;
        
		private void OnDestroy(){ _instantiated = false; }
	}    
}