using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control the texture scale of a target material over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you control the texture scale of a target material over time.")]
	[FeedbackPath("Renderer/Texture Scale")]
	public class MMFeedbackTextureScale : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }

		[Header("Material")]
		/// the renderer on which to change texture scale on
		[Tooltip("the renderer on which to change texture scale on")]
		public Renderer TargetRenderer;
		/// the material index
		[Tooltip("the material index")]
		public int MaterialIndex = 0;
		/// the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true
		[Tooltip("the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true")]
		public string MaterialPropertyName = "_MainTex_ST";
		/// whether the feedback should affect the material instantly or over a period of time
		[Tooltip("whether the feedback should affect the material instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the material should change over time
		[Tooltip("how long the material should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not the values should be relative 
		[Tooltip("whether or not the values should be relative")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
		public bool UseMaterialPropertyBlocks = false;

		[Header("Intensity")]
		/// the curve to tween the scale on
		[Tooltip("the curve to tween the scale on")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the scale curve's 0 to
		[Tooltip("the value to remap the scale curve's 0 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Vector2 RemapZero = Vector2.zero;
		/// the value to remap the scale curve's 1 to
		[Tooltip("the value to remap the scale curve's 1 to")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Vector2 RemapOne = Vector2.one;
		/// the value to move the intensity to in instant mode
		[Tooltip("the value to move the intensity to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Vector2 InstantScale;

		protected Vector2 _initialValue;
		protected Coroutine _coroutine;
		protected Vector2 _newValue;
		protected MaterialPropertyBlock _propertyBlock;
		protected Vector4 _propertyBlockVector;

		/// the duration of this feedback is the duration of the transition
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// <summary>
		/// On init we store our texture scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (UseMaterialPropertyBlocks)
			{
				_propertyBlock = new MaterialPropertyBlock();
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).w;
				_propertyBlockVector.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).z;
				_initialValue.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
				_initialValue.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;    
			}
			else
			{
				_initialValue = TargetRenderer.materials[MaterialIndex].GetTextureScale(MaterialPropertyName);    
			}
		}

		/// <summary>
		/// On Play we modify our texture scale
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            
			switch (Mode)
			{
				case Modes.Instant:      
					ApplyValue(InstantScale * intensityMultiplier);
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(TransitionCo(intensityMultiplier));

					break;
			}
		}

		/// <summary>
		/// This coroutine will animate the target material's scale over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator TransitionCo(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetMaterialValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetMaterialValues(FinalNormalizedTime, intensityMultiplier);
			IsPlaying = true;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Applies the scale to the target material
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetMaterialValues(float time, float intensityMultiplier)
		{
			_newValue.x = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.x, RemapOne.x);
			_newValue.y = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.y, RemapOne.y);

			if (RelativeValues)
			{
				_newValue += _initialValue;
			}

			ApplyValue(_newValue * intensityMultiplier);
		}

		/// <summary>
		/// Applies the specified value to the material
		/// </summary>
		/// <param name="newValue"></param>
		protected virtual void ApplyValue(Vector2 newValue)
		{
			if (UseMaterialPropertyBlocks)
			{
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.x = newValue.x;
				_propertyBlockVector.y = newValue.y;
				_propertyBlock.SetVector(MaterialPropertyName, _propertyBlockVector);
				TargetRenderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
			}
			else
			{
				TargetRenderer.materials[MaterialIndex].SetTextureScale(MaterialPropertyName, newValue);    
			}
		}

		/// <summary>
		/// Stops this feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			StopCoroutine(_coroutine);
			_coroutine = null;
		}
	}
}