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
	public class MMF_PlayerConfiguration : ScriptableObject
	{
		private static MMF_PlayerConfiguration _instance;
		private static bool _instantiated;
        
		/// <summary>
		/// Singleton pattern
		/// </summary>
		public static MMF_PlayerConfiguration Instance
		{
			get
			{
				if (_instantiated)
				{
					return _instance;
				}
                
				string assetName = typeof(MMF_PlayerConfiguration).Name;
                
				MMF_PlayerConfiguration loadedAsset = Resources.Load<MMF_PlayerConfiguration>("MMF_PlayerConfiguration");
				_instance = loadedAsset;    
				_instantiated = true;
                
				return _instance;
			}
		}
        
		[Header("Help settings")]
		/// if this is true, inspector tips will be shown for MMFeedbacks
		public bool ShowInspectorTips = true;
		/// if this is true, when exiting play mode when KeepPlaymodeChanges is active, it'll turn off automatically, otherwise it'll remain on
		public bool AutoDisableKeepPlaymodeChanges = true;
		/// if this is true, when exiting play mode when KeepPlaymodeChanges is active, it'll turn off automatically, otherwise it'll remain on
		public bool InspectorGroupsExpandedByDefault = true;


        
		private void OnDestroy(){ _instantiated = false; }
	}    
}