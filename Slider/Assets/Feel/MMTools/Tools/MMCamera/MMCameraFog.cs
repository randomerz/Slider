using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple class used to store fog properties
	/// </summary>
	[Serializable]
	public class FogSettings
	{
		public bool FogEnabled = true;
		public Color FogColor = Color.white;
		public float FogDensity = 0.01f;
		public UnityEngine.FogMode FogMode = FogMode.ExponentialSquared;
	}

	/// <summary>
	/// Add this class to a camera and it will override fog settings when active
	/// </summary>
	[ExecuteAlways]
	public class MMCameraFog : MonoBehaviour
	{
		/// the settings to use to override fog settings 
		public FogSettings Settings;

		protected FogSettings _previousSettings;

		protected void Awake()
		{
			_previousSettings = new FogSettings();
		}

		/// <summary>
		/// On pre render we store our current fog settings and override them
		/// </summary>
		protected virtual void OnPreRender()
		{
			_previousSettings.FogEnabled = RenderSettings.fog;
			_previousSettings.FogColor = RenderSettings.fogColor;
			_previousSettings.FogDensity = RenderSettings.fogDensity;
			_previousSettings.FogMode = RenderSettings.fogMode;

			RenderSettings.fog = Settings.FogEnabled;
			RenderSettings.fogColor = Settings.FogColor;
			RenderSettings.fogDensity = Settings.FogDensity;
			RenderSettings.fogMode = Settings.FogMode;
		}

		/// <summary>
		/// On post render we restore fog settings
		/// </summary>
		protected virtual void OnPostRender()
		{
			RenderSettings.fog = _previousSettings.FogEnabled;
			RenderSettings.fogColor = _previousSettings.FogColor;
			RenderSettings.fogDensity = _previousSettings.FogDensity;
			RenderSettings.fogMode = _previousSettings.FogMode;
		}
	}
}