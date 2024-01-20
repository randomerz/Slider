#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(Camera))]
	/// <summary>
	/// Forces an aspect ratio on a camera
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Camera/MMCameraAspectRatio")]
	public class MMCameraAspectRatio : MonoBehaviour 
	{
		public enum Modes { Fixed, ScreenRatio }

		[Header("Camera")]
		/// the camera to change the aspect ratio on
		[Tooltip("the camera to change the aspect ratio on")]
		public Camera TargetCamera;
		/// the mode of choice, fixed will force a specified ratio, while ScreenRatio will adapt the camera's aspect to the current screen ratio
		[Tooltip("the mode of choice, fixed will force a specified ratio, while ScreenRatio will adapt the camera's aspect to the current screen ratio")]
		public Modes Mode = Modes.Fixed;
		/// in fixed mode, the ratio to apply to the camera
		[Tooltip("in fixed mode, the ratio to apply to the camera")]
		[MMEnumCondition("Mode", (int)Modes.Fixed)]
		public Vector2 FixedAspectRatio = Vector2.zero;

		[Header("Automation")]
		/// whether or not to apply the ratio automatically on Start
		[Tooltip("whether or not to apply the ratio automatically on Start")]
		public bool ApplyAspectRatioOnStart = true;
		/// whether or not to apply the ratio automatically on enable
		[Tooltip("whether or not to apply the ratio automatically on enable")]
		public bool ApplyAspectRatioOnEnable = false;

		[Header("Debug")] 
		[MMInspectorButton("ApplyAspectRatio")]
		public bool ApplyAspectRatioButton;
		
		protected float _defaultAspect = 16f / 9f;

		/// <summary>
		/// On enable we apply our aspect ratio if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ApplyAspectRatioOnEnable) { ApplyAspectRatio(); }
		}

		/// <summary>
		/// On start we apply our aspect ratio if needed
		/// </summary>
		protected virtual void Start()
		{
			if (ApplyAspectRatioOnStart) { ApplyAspectRatio(); }
		}

		/// <summary>
		/// Applies the specified aspect ratio
		/// </summary>
		public virtual void ApplyAspectRatio()
		{
			if (TargetCamera == null)
			{
				return;
			}

			float newAspectRatio = _defaultAspect;
			float ratioX = 1f;
			float ratioY = 1f;
			switch (Mode)
			{
				case Modes.Fixed:
					ratioX = FixedAspectRatio.x;
					ratioY = FixedAspectRatio.y;
					break;
				case Modes.ScreenRatio:
					#if UNITY_EDITOR
					string[] res = UnityStats.screenRes.Split('x');
					ratioX = int.Parse(res[0]);
					ratioY = int.Parse(res[1]);
					#else
						ratioX = Screen.width;
						ratioY = Screen.height;
					#endif
					
					break;
			}
			newAspectRatio = ratioY != 0f ? ratioX / ratioY : _defaultAspect;
			TargetCamera.aspect = newAspectRatio;
		}
		
	}
}