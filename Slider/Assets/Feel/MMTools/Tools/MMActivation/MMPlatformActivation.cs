using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to a gameobject, and it'll enable/disable it based on platform context, using conditional defintions to do so
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Activation/MMPlatformActivation")]
	public class MMPlatformActivation : MonoBehaviour
	{
		/// the possible times at which this script can run
		public enum ExecutionTimes { Awake, Start, OnEnable }
		public enum PlatformActions { DoNothing, Disable }
        
		[Header("Settings")]
		/// the selected execution time
		public ExecutionTimes ExecutionTime = ExecutionTimes.Awake;
		/// whether or not this should output a debug line in the console
		public bool DebugToTheConsole = false;

		[Header("Desktop")]
		/// whether or not this gameobject should be active on Windows
		public PlatformActions UNITY_STANDALONE_WIN = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on OSX
		public PlatformActions UNITY_STANDALONE_OSX = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Linux
		public PlatformActions UNITY_STANDALONE_LINUX = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on standalone
		public PlatformActions UNITY_STANDALONE = PlatformActions.DoNothing;

		[Header("Mobile")]
		/// whether or not this gameobject should be active on iOS
		public PlatformActions UNITY_IOS = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on iPhone
		public PlatformActions UNITY_IPHONE = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Android
		public PlatformActions UNITY_ANDROID = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Tizen
		public PlatformActions UNITY_TIZEN = PlatformActions.DoNothing;

		[Header("Console")]
		/// whether or not this gameobject should be active on Wii
		public PlatformActions UNITY_WII = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on PS4
		public PlatformActions UNITY_PS4 = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on XBoxOne
		public PlatformActions UNITY_XBOXONE = PlatformActions.DoNothing;

		[Header("Others")]
		/// whether or not this gameobject should be active on WebGL
		public PlatformActions UNITY_WEBGL = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Lumin
		public PlatformActions UNITY_LUMIN = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on TVOS
		public PlatformActions UNITY_TVOS = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on WSA
		public PlatformActions UNITY_WSA = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Facebook
		public PlatformActions UNITY_FACEBOOK = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Ads
		public PlatformActions UNITY_ADS = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active on Analytics
		public PlatformActions UNITY_ANALYTICS = PlatformActions.DoNothing;

		[Header("Active in Editor")]
		/// whether or not this gameobject should be active in Editor
		public PlatformActions UNITY_EDITOR = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active in Editor on Windows
		public PlatformActions UNITY_EDITOR_WIN = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active in Editor on OSX
		public PlatformActions UNITY_EDITOR_OSX = PlatformActions.DoNothing;
		/// whether or not this gameobject should be active in Editor on Linux
		public PlatformActions UNITY_EDITOR_LINUX = PlatformActions.DoNothing;

		/// <summary>
		/// On Enable, processes the state if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ExecutionTime == ExecutionTimes.OnEnable)
			{
				Process();
			}
		}

		/// <summary>
		/// On Awake, processes the state if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (ExecutionTime == ExecutionTimes.Awake)
			{
				Process();
			}            
		}

		/// <summary>
		/// On Start, processes the state if needed
		/// </summary>
		protected virtual void Start()
		{
			if (ExecutionTime == ExecutionTimes.Start)
			{
				Process();
			}            
		}

		/// <summary>
		/// Enables or disables the object based on current platform
		/// </summary>
		protected virtual void Process()
		{
			// DESKTOP ----------------------------------------------------------------------------------

			#if UNITY_STANDALONE_WIN
			DisableIfNeeded(UNITY_STANDALONE_WIN, "Windows");
			#endif

			#if UNITY_STANDALONE_OSX
                DisableIfNeeded(UNITY_STANDALONE_OSX, "OSX");
			#endif

			#if UNITY_STANDALONE_LINUX
                DisableIfNeeded(UNITY_STANDALONE_LINUX, "Linux");
			#endif

			#if UNITY_STANDALONE
			DisableIfNeeded(UNITY_STANDALONE, "Standalone");
			#endif

			// MOBILE ----------------------------------------------------------------------------------

			#if UNITY_IOS
                DisableIfNeeded(UNITY_IOS, "iOS");
			#endif

			#if UNITY_IPHONE
                DisableIfNeeded(UNITY_IPHONE, "iPhone");
			#endif

			#if UNITY_ANDROID
                DisableIfNeeded(UNITY_ANDROID, "Android");
			#endif

			#if UNITY_TIZEN
                DisableIfNeeded(UNITY_TIZEN, "Tizen");
			#endif

			// CONSOLE ----------------------------------------------------------------------------------

			#if UNITY_WII
                DisableIfNeeded(UNITY_WII, "Wii");
			#endif

			#if UNITY_PS4
                DisableIfNeeded(UNITY_PS4, "PS4");
			#endif

			#if UNITY_XBOXONE
                DisableIfNeeded(UNITY_XBOXONE, "XBoxOne");
			#endif

			// CONSOLE ----------------------------------------------------------------------------------

			#if UNITY_WEBGL
                DisableIfNeeded(UNITY_WEBGL, "WebGL");
			#endif

			#if UNITY_LUMIN
                DisableIfNeeded(UNITY_LUMIN, "Lumin");
			#endif

			#if UNITY_TVOS
                DisableIfNeeded(UNITY_TVOS, "TV OS");
			#endif

			#if UNITY_WSA
                DisableIfNeeded(UNITY_WSA, "WSA");
			#endif

			#if UNITY_FACEBOOK
                DisableIfNeeded(UNITY_FACEBOOK, "Facebook");
			#endif

			#if UNITY_ADS
                DisableIfNeeded(UNITY_ADS, "Ads");
			#endif

			#if UNITY_ANALYTICS
                DisableIfNeeded(UNITY_ANALYTICS, "Analytics");
			#endif

			// EDITOR ----------------------------------------------------------------------------------

			#if UNITY_EDITOR
			DisableIfNeeded(UNITY_EDITOR, "Editor");
			#endif

			#if UNITY_EDITOR_WIN
			DisableIfNeeded(UNITY_EDITOR_WIN, "Editor Windows");
			#endif

			#if UNITY_EDITOR_OSX
                DisableIfNeeded(UNITY_EDITOR_OSX, "Editor OSX");
			#endif

			#if UNITY_EDITOR_LINUX
                DisableIfNeeded(UNITY_EDITOR_LINUX, "Editor Linux");
			#endif
		}

		/// <summary>
		/// Disables the object if needed, and outputs a debug log if requested
		/// </summary>
		/// <param name="platform"></param>
		/// <param name="platformName"></param>
		protected virtual void DisableIfNeeded(PlatformActions platform, string platformName)
		{
			if (this.gameObject.activeInHierarchy && (platform == PlatformActions.Disable))
			{
				this.gameObject.SetActive(false);
				if (DebugToTheConsole)
				{
					Debug.LogFormat(this.gameObject.name + " got disabled via MMPlatformActivation, platform : " + platformName + ".");
				}
			}
		}
	}
}