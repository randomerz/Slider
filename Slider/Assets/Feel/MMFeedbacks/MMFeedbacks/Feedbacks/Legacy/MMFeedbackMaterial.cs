using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the material of the target renderer everytime it's played.")]
	[FeedbackPath("Renderer/Material")]
	public class MMFeedbackMaterial : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return (InterpolateTransition) ? TransitionDuration : 0f; } set { if (InterpolateTransition) { TransitionDuration = value; } } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif

		/// the possible methods to switch materials
		public enum Methods { Sequential, Random }

		[Header("Material")]
		/// the renderer to change material on
		[Tooltip("the renderer to change material on")]
		public Renderer TargetRenderer;
		/// the list of material indexes we want to change on the target renderer. If left empty, will only target the material at index 0 
		[FormerlySerializedAs("MaterialIndexes")] [Tooltip("the list of material indexes we want to change on the target renderer. If left empty, will only target the material at index 0")]
		public int[] RendererMaterialIndexes;
        
		[Header("Material Change")]
		/// the selected method
		[Tooltip("the selected method")]
		public Methods Method;
		/// whether or not the sequential order should loop
		[MMFEnumCondition("Method", (int)Methods.Sequential)]
		[Tooltip("whether or not the sequential order should loop")]
		public bool Loop = true;
		/// whether or not to always pick a new material in random mode
		[MMFEnumCondition("Method", (int)Methods.Random)]        
		[Tooltip("whether or not to always pick a new material in random mode")]
		public bool AlwaysNewMaterial = true;
		/// the initial index to start with
		[Tooltip("the initial index to start with")]
		public int InitialIndex = 0;
		/// the list of materials to pick from
		[Tooltip("the list of materials to pick from")]
		public List<Material> Materials;

		[Header("Interpolation")]
		/// whether or not to interpolate between 2 materials
		/// IMPORTANT : this will only work for materials that share the same shader and texture (see https://docs.unity3d.com/ScriptReference/Material.Lerp.html)
		public bool InterpolateTransition = false;
		/// the duration of the interpolation, in seconds
		public float TransitionDuration = 1f;
		/// the animation curve to interpolate the transition on
		public AnimationCurve TransitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

		public virtual float GetTime() { return (Timing.TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (Timing.TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		protected int _currentIndex;
		protected float _startedAt;
		protected Coroutine[] _coroutines;
		protected Material[] _tempMaterials;

		/// <summary>
		/// On init, grabs the current index
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			_currentIndex = InitialIndex;
			_tempMaterials = new Material[TargetRenderer.materials.Length];
			if (RendererMaterialIndexes == null)
			{
				RendererMaterialIndexes = new int[1];
			}
			if (RendererMaterialIndexes.Length == 0)
			{
				RendererMaterialIndexes = new int[1];
				RendererMaterialIndexes[0] = 0;
			}
			_coroutines = new Coroutine[RendererMaterialIndexes.Length];
		}

		/// <summary>
		/// On play feedback, we change the material if possible
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Materials.Count == 0)
			{
				Debug.LogError("[MMFeedbackMaterial on " + this.name + "] The Materials array is empty.");
				return;
			}

			int newIndex = DetermineNextIndex();

			if (Materials[newIndex] == null)
			{
				Debug.LogError("[MMFeedbackMaterial on " + this.name + "] Attempting to switch to a null material.");
				return;
			}

			if (InterpolateTransition)
			{
				for (int i = 0; i < RendererMaterialIndexes.Length; i++)
				{
					_coroutines[i] = StartCoroutine(TransitionMaterial(TargetRenderer.materials[RendererMaterialIndexes[i]], Materials[newIndex], RendererMaterialIndexes[i]));
				}
			}
			else
			{
				ApplyMaterial(Materials[newIndex]);
			}            
		}

		/// <summary>
		/// Applies the new material to all indexes
		/// </summary>
		/// <param name="material"></param>
		protected virtual void ApplyMaterial(Material material)
		{
			_tempMaterials = TargetRenderer.materials;
			for (int i = 0; i < RendererMaterialIndexes.Length; i++)
			{
				_tempMaterials[RendererMaterialIndexes[i]] = material;
			}
			TargetRenderer.materials = _tempMaterials;
		}

		/// <summary>
		/// Lerps to destination material for all indexes
		/// </summary>
		/// <param name="fromMaterial"></param>
		/// <param name="toMaterial"></param>
		/// <param name="t"></param>
		/// <param name="materialIndex"></param>
		protected virtual void LerpMaterial(Material fromMaterial, Material toMaterial, float t, int materialIndex)
		{
			_tempMaterials = TargetRenderer.materials;
			for (int i = 0; i < RendererMaterialIndexes.Length; i++)
			{
				_tempMaterials[materialIndex].Lerp(fromMaterial, toMaterial, t);
			}
			TargetRenderer.materials = _tempMaterials;
		}
        
		/// <summary>
		/// A coroutine used to interpolate between materials
		/// </summary>
		/// <param name="originalMaterial"></param>
		/// <param name="newMaterial"></param>
		/// <returns></returns>
		protected virtual IEnumerator TransitionMaterial(Material originalMaterial, Material newMaterial, int materialIndex)
		{
			IsPlaying = true;
			_startedAt = GetTime();
			while (GetTime() - _startedAt < TransitionDuration)
			{
				float time = MMFeedbacksHelpers.Remap(GetTime() - _startedAt, 0f, TransitionDuration, 0f, 1f);
				float t = TransitionCurve.Evaluate(time);
                
				LerpMaterial(originalMaterial, newMaterial, t, materialIndex);
				yield return null;
			}
			float finalt = TransitionCurve.Evaluate(1f);
			LerpMaterial(originalMaterial, newMaterial, finalt, materialIndex);
			IsPlaying = false;
		}

		/// <summary>
		/// Determines the new material to pick
		/// </summary>
		/// <returns></returns>
		protected virtual int DetermineNextIndex()
		{
			switch(Method)
			{
				case Methods.Random:
					int random = Random.Range(0, Materials.Count);
					if (AlwaysNewMaterial)
					{
						while (_currentIndex == random)
						{
							random = Random.Range(0, Materials.Count);
						}
					}
					_currentIndex = random;
					return _currentIndex;                    

				case Methods.Sequential:
					_currentIndex++;
					if (_currentIndex >= Materials.Count)
					{
						_currentIndex = Loop ? 0 : _currentIndex;
					}
					return _currentIndex;
			}
			return 0;
		}

		/// <summary>
		/// Stops the transition on stop if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && FeedbackTypeAuthorized && (_coroutines != null))
			{
				IsPlaying = false;
				for (int i = 0; i < RendererMaterialIndexes.Length; i++)
				{
					if (_coroutines[i] != null)
					{
						StopCoroutine(_coroutines[i]);    
					}
					_coroutines[i] = null;    
				}
			}
		}
	}
}