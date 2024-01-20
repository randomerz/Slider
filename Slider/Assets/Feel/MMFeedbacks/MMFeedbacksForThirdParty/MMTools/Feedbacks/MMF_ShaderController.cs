using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control values on a target ShaderController, letting you modify the behaviour and aspect of a shader driven material at runtime
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you trigger a one time play on a target ShaderController.")]
	[FeedbackPath("Renderer/ShaderController")]
	public class MMF_ShaderController : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the different possible modes 
		public enum Modes { OneTime, ToDestination }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetShaderController == null); }
		public override string RequiredTargetText { get { return TargetShaderController != null ? TargetShaderController.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetShaderController be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetShaderController = FindAutomatedTarget<ShaderController>();

		[MMFInspectorGroup("Shader Controller", true, 37, true)]
		/// the mode this controller is in
		[Tooltip("the mode this controller is in")]
		public Modes Mode = Modes.OneTime;
		/// the float controller to trigger a one time play on
		[Tooltip("the float controller to trigger a one time play on")]
		public ShaderController TargetShaderController;
		/// an optional list of float controllers to trigger a one time play on
		[Tooltip("an optional list of float controllers to trigger a one time play on")]
		public List<ShaderController> TargetShaderControllerList;
		/// whether this should revert to original at the end
		[Tooltip("whether this should revert to original at the end")]
		public bool RevertToInitialValueAfterEnd = false;
		/// the duration of the One Time shake
		[Tooltip("the duration of the One Time shake")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("the amplitude of the One Time shake (this will be multiplied by the curve's height)")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("the low value to remap the normalized curve value to")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("the high value to remap the normalized curve value to")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("the curve to apply to the one time shake")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the new value towards which to move the current value
		[Tooltip("the new value towards which to move the current value")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationValue = 1f;
		/// the duration over which to interpolate the target value
		[Tooltip("the duration over which to interpolate the target value")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationDuration = 1f;
		/// the color to aim for (when targetting a Color property
		[Tooltip("the color to aim for (when targetting a Color property")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Color ToDestinationColor = Color.red;
		/// the curve over which to interpolate the value
		[Tooltip("the curve over which to interpolate the value")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the duration of this feedback is the duration of the one time hit
		public override float FeedbackDuration
		{
			get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); } 
			set { OneTimeDuration = value; ToDestinationDuration = value; }
		}

		protected float _oneTimeDurationStorage;
		protected float _oneTimeAmplitudeStorage;
		protected float _oneTimeRemapMinStorage;
		protected float _oneTimeRemapMaxStorage;
		protected AnimationCurve _oneTimeCurveStorage;
		protected float _toDestinationValueStorage;
		protected float _toDestinationDurationStorage;
		protected AnimationCurve _toDestinationCurveStorage;
		protected bool _revertToInitialValueAfterEndStorage;

		/// <summary>
		/// On init we grab our initial controller values
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			if (Active && (TargetShaderController != null))
			{
				_oneTimeDurationStorage = TargetShaderController.OneTimeDuration;
				_oneTimeAmplitudeStorage = TargetShaderController.OneTimeAmplitude;
				_oneTimeCurveStorage = TargetShaderController.OneTimeCurve;
				_oneTimeRemapMinStorage = TargetShaderController.OneTimeRemapMin;
				_oneTimeRemapMaxStorage = TargetShaderController.OneTimeRemapMax;
				_toDestinationCurveStorage = TargetShaderController.ToDestinationCurve;
				_toDestinationDurationStorage = TargetShaderController.ToDestinationDuration;
				_toDestinationValueStorage = TargetShaderController.ToDestinationValue;
				_revertToInitialValueAfterEndStorage = TargetShaderController.RevertToInitialValueAfterEnd;
			}
		}

		/// <summary>
		/// On play we trigger a OneTime or ToDestination play on our shader controller
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetShaderController == null))
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            
			PerformPlay(TargetShaderController, intensityMultiplier);     

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				PerformPlay(shaderController, intensityMultiplier);     
			}    
		}

		protected virtual void PerformPlay(ShaderController shaderController, float intensityMultiplier)
		{
			shaderController.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;
			if (Mode == Modes.OneTime)
			{
				shaderController.OneTimeDuration = FeedbackDuration;
				shaderController.OneTimeAmplitude = OneTimeAmplitude;
				shaderController.OneTimeCurve = OneTimeCurve;
				if (NormalPlayDirection)
				{
					shaderController.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
					shaderController.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;    
				}
				else
				{
					shaderController.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
					shaderController.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;
				}
				shaderController.OneTime();
			}
			if (Mode == Modes.ToDestination)
			{
				shaderController.ToColor = ToDestinationColor;
				shaderController.ToDestinationCurve = ToDestinationCurve;
				shaderController.ToDestinationDuration = FeedbackDuration;
				shaderController.ToDestinationValue = ToDestinationValue;
				shaderController.ToDestination();
			}   
		}
        
		/// <summary>
		/// Stops this feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			if (TargetShaderController != null)
			{
				TargetShaderController.Stop();
			}

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				shaderController.Stop();
			}
		}

		/// <summary>
		/// On reset we restore our initial values
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			if (Active && FeedbackTypeAuthorized && (TargetShaderController != null))
			{
				PerformReset(TargetShaderController);
			}

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				PerformReset(shaderController);
			}
		}

		protected virtual void PerformReset(ShaderController shaderController)
		{
			shaderController.OneTimeDuration = _oneTimeDurationStorage;
			shaderController.OneTimeAmplitude = _oneTimeAmplitudeStorage;
			shaderController.OneTimeCurve = _oneTimeCurveStorage;
			shaderController.OneTimeRemapMin = _oneTimeRemapMinStorage;
			shaderController.OneTimeRemapMax = _oneTimeRemapMaxStorage;
			shaderController.ToDestinationCurve = _toDestinationCurveStorage;
			shaderController.ToDestinationDuration = _toDestinationDurationStorage;
			shaderController.ToDestinationValue = _toDestinationValueStorage;
			shaderController.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			TargetShaderController.RestoreInitialValues();     

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				shaderController.RestoreInitialValues();     
			}  
		}
	}
}