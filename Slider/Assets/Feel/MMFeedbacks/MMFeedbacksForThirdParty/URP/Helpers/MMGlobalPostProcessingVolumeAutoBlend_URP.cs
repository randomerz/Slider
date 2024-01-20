using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Rendering;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Use this class to have a global PP volume auto blend its weight on cue, between a start and end values
	/// </summary>
	#if MM_URP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMGlobalPostProcessingVolumeAutoBlend_URP")]
	public class MMGlobalPostProcessingVolumeAutoBlend_URP : MonoBehaviour
	{
		/// the possible timescales this blend can operate on
		public enum TimeScales { Scaled, Unscaled }
		/// the possible blend trigger modes 
		public enum BlendTriggerModes { OnEnable, Script }

		[Header("Blend")]
		/// the trigger mode for this MMGlobalPostProcessingVolumeAutoBlend
		/// Start : will play automatically on enable
		public BlendTriggerModes BlendTriggerMode = BlendTriggerModes.OnEnable;
		/// the duration of the blend (in seconds)
		public float BlendDuration = 1f;
		/// the curve to use to blend
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));

		[Header("Weight")]
		/// the weight at the start of the blend
		[Range(0f, 1f)]
		public float InitialWeight = 0f;
		/// the desired weight at the end of the blend
		[Range(0f, 1f)]
		public float FinalWeight = 1f;

		[Header("Behaviour")]
		/// the timescale to operate on
		public TimeScales TimeScale = TimeScales.Unscaled;
		/// whether or not the associated volume should be disabled at 0
		public bool DisableVolumeOnZeroWeight = true;
		/// whether or not this blender should disable itself at 0
		public bool DisableSelfAfterEnd = true;
		/// whether or not this blender can be interrupted
		public bool Interruptable = true;
		/// whether or not this blender should pick the current value as its starting point
		public bool StartFromCurrentValue = true;

		[Header("Tests")]
		/// test blend button
		[MMFInspectorButton("Blend")]
		public bool TestBlend;
		/// test blend back button
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
		#if MM_URP
		protected Volume _volume;

		/// <summary>
		/// On Awake we store our volume
		/// </summary>
		protected virtual void Awake()
		{
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.weight = InitialWeight;
		}
		#endif

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
			#if MM_URP
			if (_blending && !Interruptable)
			{
				return;
			}
			_initial = StartFromCurrentValue ? _volume.weight : InitialWeight;
			_destination = FinalWeight;
			StartBlending();
			#endif
		}

		/// <summary>
		/// Blends the volume's weight from the final value to the initial one
		/// </summary>
		public virtual void BlendBack()
		{
			#if MM_URP
			if (_blending && !Interruptable)
			{
				return;
			}
			_initial = StartFromCurrentValue ? _volume.weight : FinalWeight;
			_destination = InitialWeight;
			StartBlending();
			#endif
		}

		/// <summary>
		/// Internal method used to start blending
		/// </summary>
		protected virtual void StartBlending()
		{
			#if MM_URP
			_startTime = GetTime();
			_blending = true;
			this.enabled = true;
			if (DisableVolumeOnZeroWeight)
			{
				_volume.enabled = true;
			}
			#endif
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
            
			#if MM_URP
			float timeElapsed = (GetTime() - _startTime);
			if (timeElapsed < BlendDuration)
			{                
				float remapped = MMFeedbacksHelpers.Remap(timeElapsed, 0f, BlendDuration, 0f, 1f);
				_volume.weight = Mathf.LerpUnclamped(_initial, _destination, Curve.Evaluate(remapped));
			}
			else
			{
				// after end is reached
				_volume.weight = _destination;
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
			#endif
		}
	
		public virtual void RestoreInitialValues()
		{
			#if MM_URP
			_volume.weight = _initial;
			#endif
		}
	}
}