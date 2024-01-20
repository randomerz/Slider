using System;
using UnityEngine;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.Tools
{	
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MMHealthBar),true)]
	/// <summary>
	/// Custom editor for health bars (mostly a switch for prefab based / drawn bars
	/// </summary>
	public class HealthBarEditor : Editor 
	{
		public MMHealthBar HealthBarTarget 
		{ 
			get 
			{ 
				return (MMHealthBar)target;
			}
		} 

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			switch (HealthBarTarget.HealthBarType)
			{
				case MMHealthBar.HealthBarTypes.Prefab:
					Editor.DrawPropertiesExcluding(serializedObject, new string[] {"TargetProgressBar", "NestDrawnHealthBar", "Billboard", "FollowTargetMode", "Size","BackgroundPadding", "SortingLayerName", "InitialRotationAngles", "ForegroundColor", "DelayedColor", "BorderColor", "BackgroundColor", "Delay", "LerpFrontBar", "LerpFrontBarSpeed", "LerpDelayedBar", "LerpDelayedBarSpeed", "BumpScaleOnChange", "BumpDuration", "BumpAnimationCurve" });
					break;
				case MMHealthBar.HealthBarTypes.Drawn:
					Editor.DrawPropertiesExcluding(serializedObject, new string[] {"TargetProgressBar", "HealthBarPrefab" });
					break;
				case MMHealthBar.HealthBarTypes.Existing:
					Editor.DrawPropertiesExcluding(serializedObject, new string[] {"HealthBarPrefab", "NestDrawnHealthBar", "Billboard", "FollowTargetMode", "Size","BackgroundPadding", "SortingLayerName", "InitialRotationAngles", "ForegroundColor", "DelayedColor", "BorderColor", "BackgroundColor", "Delay", "LerpFrontBar", "LerpFrontBarSpeed", "LerpDelayedBar", "LerpDelayedBarSpeed", "BumpScaleOnChange", "BumpDuration", "BumpAnimationCurve" });
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}

	}
}