using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to any object and it'll set the target frame rate and vsync count. Note that vsync count must be 0 for the target FPS to work.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Performance/MMFPSUnlock")]
	public class MMFPSUnlock : MonoBehaviour
	{
		/// the target FPS you want the game to run at, that's up to how many times Update will run every second
		[Tooltip("the target FPS you want the game to run at, that's up to how many times Update will run every second")]
		public int TargetFPS;
		/// the number of frames to wait before rendering the next one. 0 will render every frame, 1 will render every 2 frames, 5 will render every 5 frames, etc
		[Tooltip("the number of frames to wait before rendering the next one. 0 will render every frame, 1 will render every 2 frames, 5 will render every 5 frames, etc")]
		public int RenderFrameInterval = 0;
		[Range(0,2)]
		/// whether vsync should be enabled or not (on a 60Hz screen, 1 : 60fps, 2 : 30fps, 0 : don't wait for vsync)
		[Tooltip("whether vsync should be enabled or not (on a 60Hz screen, 1 : 60fps, 2 : 30fps, 0 : don't wait for vsync)")]
		public int VSyncCount = 0;

		/// <summary>
		/// On start we change our target fps and vsync settings
		/// </summary>
		protected virtual void Start()
		{
			UpdateSettings();
		}	
        
		/// <summary>
		/// When a value gets changed in the editor, we update our settings
		/// </summary>
		protected virtual void OnValidate()
		{
			UpdateSettings();
		}

		/// <summary>
		/// Updates the target frame rate value and vsync count setting
		/// </summary>
		protected virtual void UpdateSettings()
		{
			QualitySettings.vSyncCount = VSyncCount;
			Application.targetFrameRate = TargetFPS;
			OnDemandRendering.renderFrameInterval = RenderFrameInterval;
		}
	}
}