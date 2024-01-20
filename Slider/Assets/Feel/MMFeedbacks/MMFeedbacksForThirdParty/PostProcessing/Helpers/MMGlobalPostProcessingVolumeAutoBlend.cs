using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_POSTPROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif 

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Use this class to have a global PP volume auto blend its weight on cue, between a start and end values
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMGlobalPostProcessingVolumeAutoBlend")]
	#if MM_POSTPROCESSING
	[RequireComponent(typeof(PostProcessVolume))]
	#endif
	public class MMGlobalPostProcessingVolumeAutoBlend : MonoBehaviour
	{
		/// the possible timescales this blend can operate on
		public enum TimeScales { Scaled, Unscaled }
		/// the possible blend trigger modes 
		public enum BlendTriggerModes { OnEnable, Script }

		[Header("Blend")]
		/// the trigger mode for this MMGlobalPostProcessingVolumeAutoBlend
		/// Start : will play automatically on enable
		[Tooltip("the trigger mode for this MMGlobalPostProcessingVolumeAutoBlend")]
		public BlendTriggerModes BlendTriggerMode = BlendTriggerModes.OnEnable;
		/// the duration of the blend (in seconds)
		[Tooltip("the duration of the blend (in seconds)")]
		public float BlendDuration = 1f;
		/// the curve to use to blend
		[Tooltip("the curve to use to blend")]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));

		[Header("Weight")]
		/// the weight at the start of the blend
		[Tooltip("the weight at the start of the blend")]
		[Range(0f, 1f)]
		public float InitialWeight = 0f;
		/// the desired weight at the end of the blend
		[Tooltip("the desired weight at the end of the blend")]
		[Range(0f, 1f)]
		public float FinalWeight = 1f;

		[Header("Behaviour")]
		/// the timescale to operate on
		[Tooltip("the timescale to operate on")]
		public TimeScales TimeScale = TimeScales.Unscaled;
		/// whether or not the associated volume should be disabled at 0
		[Tooltip("whether or not the associated volume should be disabled at 0")]
		public bool DisableVolumeOnZeroWeight = true;
		/// whether or not this blender should disable itself at 0
		[Tooltip("whether or not this blender should disable itself at 0")]
		public bool DisableSelfAfterEnd = true;
		/// whether or not this blender can be interrupted
		[Tooltip("whether or not this blender can be interrupted")]
		public bool Interruptable = true;
		/// whether or not this blender should pick the current value as its starting point
		[Tooltip("whether or not this blender should pick the current value as its starting point")]
		public bool StartFromCurrentValue = true;
		/// reset to initial value on end 
		[Tooltip("reset to initial value on end ")]
		public bool ResetToInitialValueOnEnd = false;

		[Header("Tests")]
		/// test blend button
		[Tooltip("test blend button")]
		[MMFInspectorButton("Blend")]
		public bool TestBlend;
		/// test blend back button
		[Tooltip("test blend back button")]
		[MMFInspectorButton("BlendBack")]
		public bool TestBlendBackwards;

		/// <summary>
		/// Returns the correct timescale based on the chosen settings
		/// </summary>
		/// <returns></returns>
		protected float GetTime()
		{
			return (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;
		}

		protected float _initial;
		protected float _destination;
		protected float _startTime;
		protected bool _blending = false;
		#if MM_POSTPROCESSING
		protected PostProcessVolume _volume;
		
		/// <summary>
		/// On Awake we store our volume
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_POSTPROCESSING
			_volume = this.gameObject.GetComponent<PostProcessVolume>();
			_volume.weight = InitialWeight;
			#endif
		}

		/// <summary>
		/// On start we start blending if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if ((BlendTriggerMode == BlendTriggerModes.OnEnable) && !_blending)
			{
				Blend();
			}
		}

		/// <summary>
		/// Blends the volume's weight from the initial value to the final one
		/// </summary>
		public virtual void Blend()
		{
			if (_blending && !Interruptable)
			{
				return;
			}
			_initial = StartFromCurrentValue ? _volume.weight : InitialWeight;
			_destination = FinalWeight;
			StartBlending();
		}

		/// <summary>
		/// Blends the volume's weight from the final value to the initial one
		/// </summary>
		public virtual void BlendBack()
		{
			if (_blending && !Interruptable)
			{
				return;
			}
			_initial = StartFromCurrentValue ? _volume.weight : FinalWeight;
			_destination = InitialWeight;
			StartBlending();
		}

		/// <summary>
		/// Internal method used to start blending
		/// </summary>
		protected virtual void StartBlending()
		{
			_startTime = GetTime();
			_blending = true;
			this.enabled = true;
			if (DisableVolumeOnZeroWeight)
			{
				_volume.enabled = true;
			}
		}

		/// <summary>
		/// Stops any blending that may be in progress
		/// </summary>
		public virtual void StopBlending()
		{
			_blending = false;
		}

		/// <summary>
		/// On update, processes the blend if needed
		/// </summary>
		protected virtual void Update()
		{
			if (!_blending)
			{
				return;
			}

			float timeElapsed = (GetTime() - _startTime);
			if (timeElapsed < BlendDuration)
			{                
				float remapped = MMFeedbacksHelpers.Remap(timeElapsed, 0f, BlendDuration, 0f, 1f);
				_volume.weight = Mathf.LerpUnclamped(_initial, _destination, Curve.Evaluate(remapped));
			}
			else
			{
				// after end is reached
				_volume.weight = ResetToInitialValueOnEnd ? _initial : _destination;
				_blending = false;
				if (DisableVolumeOnZeroWeight && (_volume.weight == 0f))
				{
					_volume.enabled = false;
				}
				if (DisableSelfAfterEnd)
				{
					this.enabled = false;
				}
			}            
		}
	
		public virtual void RestoreInitialValues()
		{
			_volume.weight = _initial;
		}
		
		#endif
	}
}