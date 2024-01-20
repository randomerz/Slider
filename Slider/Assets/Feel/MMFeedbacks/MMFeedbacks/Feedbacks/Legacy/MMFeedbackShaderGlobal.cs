using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Turns an object active or inactive at the various stages of the feedback
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to set global properties on your shader, or enable/disable keywords.")]
	[FeedbackPath("Renderer/Shader Global")]
	public class MMFeedbackShaderGlobal : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif

		public enum Modes { SetGlobalColor, SetGlobalFloat, SetGlobalInt, SetGlobalMatrix, SetGlobalTexture, SetGlobalVector, EnableKeyword, DisableKeyword, WarmupAllShaders }

		[Header("Shader Global")]
        
		public Modes Mode = Modes.SetGlobalFloat;
        
		/// the name of the global property
		[Tooltip("the name of the global property")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor, (int)Modes.SetGlobalFloat, (int)Modes.SetGlobalInt, (int)Modes.SetGlobalMatrix, (int)Modes.SetGlobalTexture, (int)Modes.SetGlobalVector)]
		public string PropertyName = "";
		/// the name ID of the property retrieved by Shader.PropertyToID
		[Tooltip("the name ID of the property retrieved by Shader.PropertyToID")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor, (int)Modes.SetGlobalFloat, (int)Modes.SetGlobalInt, (int)Modes.SetGlobalMatrix, (int)Modes.SetGlobalTexture, (int)Modes.SetGlobalVector)]
		public int PropertyNameID = 0;
		/// a global color property for all shaders
		[Tooltip("a global color property for all shaders")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor)]
		public Color GlobalColor = Color.yellow;
		/// a global float property for all shaders
		[Tooltip("a global float property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalFloat)]
		public float GlobalFloat = 1f;
		/// a global int property for all shaders
		[Tooltip("a global int property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalInt)]
		public int GlobalInt = 1;
		/// a global matrix property for all shaders
		[Tooltip("a global matrix property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalMatrix)]
		public Matrix4x4 GlobalMatrix = Matrix4x4.identity;
		/// a global texture property for all shaders
		[Tooltip("a global texture property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalTexture)]
		public RenderTexture GlobalTexture;
		/// a global vector property for all shaders
		[Tooltip("a global vector property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalVector)]
		public Vector4 GlobalVector;
		/// a global shader keyword
		[Tooltip("a global shader keyword")] 
		[MMFEnumCondition("Mode", (int)Modes.EnableKeyword, (int)Modes.DisableKeyword)]
		public string Keyword;
        
		/// <summary>
		/// On Play we set our global shader property
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.SetGlobalColor:
					if (PropertyName == "")
					{
						Shader.SetGlobalColor(PropertyNameID, GlobalColor);
					}
					else
					{
						Shader.SetGlobalColor(PropertyName, GlobalColor);
					}
					break;
				case Modes.SetGlobalFloat:
					if (PropertyName == "")
					{
						Shader.SetGlobalFloat(PropertyNameID, GlobalFloat);
					}
					else
					{
						Shader.SetGlobalFloat(PropertyName, GlobalFloat);
					}
					break;
				case Modes.SetGlobalInt:
					if (PropertyName == "")
					{
						Shader.SetGlobalInt(PropertyNameID, GlobalInt);
					}
					else
					{
						Shader.SetGlobalInt(PropertyName, GlobalInt);
					}
					break;
				case Modes.SetGlobalMatrix:
					if (PropertyName == "")
					{
						Shader.SetGlobalMatrix(PropertyNameID, GlobalMatrix);
					}
					else
					{
						Shader.SetGlobalMatrix(PropertyName, GlobalMatrix);
					}
					break;
				case Modes.SetGlobalTexture:
					if (PropertyName == "")
					{
						Shader.SetGlobalTexture(PropertyNameID, GlobalTexture);
					}
					else
					{
						Shader.SetGlobalTexture(PropertyName, GlobalTexture);
					}
					break;
				case Modes.SetGlobalVector:
					if (PropertyName == "")
					{
						Shader.SetGlobalVector(PropertyNameID, GlobalVector);
					}
					else
					{
						Shader.SetGlobalVector(PropertyName, GlobalVector);
					}
					break;
				case Modes.EnableKeyword:
					Shader.EnableKeyword(Keyword);
					break;
				case Modes.DisableKeyword:
					Shader.DisableKeyword(Keyword);
					break;
				case Modes.WarmupAllShaders:
					Shader.WarmupAllShaders();
					break;
			}
		}
	}
}