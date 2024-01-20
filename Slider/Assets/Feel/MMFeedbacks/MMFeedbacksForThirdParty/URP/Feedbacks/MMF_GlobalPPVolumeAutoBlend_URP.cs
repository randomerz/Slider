using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you pilot a Global PostProcessing Volume AutoBlend URP component. A GPPVAB component is placed on a PostProcessing Volume, and will let you control and blend its weight over time on demand.    
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you pilot a Global PostProcessing Volume AutoBlend URP component. A GPPVAB component is placed on a PostProcessing Volume, and will let you control and blend its weight over time on demand.")]
	#if MM_URP
	[FeedbackPath("PostProcess/Global PP Volume Auto Blend URP")]
	#endif
	public class MMF_GlobalPPVolumeAutoBlend_URP : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes for this feedback :
		/// - default : will let you trigger Blend() and BlendBack() on the blender
		/// - override : lets you specify new initial, final, duration and curve values on the blender, and triggers a Blend()
		public enum Modes { Default, Override }
		/// the possible actions when in Default mode
		public enum Actions { Blend, BlendBack }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetAutoBlend == null); }
		public override string RequiredTargetText { get { return TargetAutoBlend != null ? TargetAutoBlend.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetCanvasGroup be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetAutoBlend = FindAutomatedTarget<MMGlobalPostProcessingVolumeAutoBlend_URP>();

		/// defines the duration of the feedback
		public override float FeedbackDuration
		{
			get
			{
				if (Mode == Modes.Override)
				{
					return ApplyTimeMultiplier(BlendDuration);
				}
				else
				{
					if (TargetAutoBlend == null)
					{
						return 0.1f;
					}
					else
					{
						return ApplyTimeMultiplier(TargetAutoBlend.BlendDuration);
					}
				}
			}
			set
			{
				BlendDuration = value;
				if (TargetAutoBlend != null)
				{
					TargetAutoBlend.BlendDuration = value;
				}
			}
		}
               
		[MMFInspectorGroup("PostProcess Volume Blend", true, 22, true)]
		/// the target auto blend to pilot with this feedback
		public MMGlobalPostProcessingVolumeAutoBlend_URP TargetAutoBlend;
		/// the chosen mode
		public Modes Mode = Modes.Default;
		/// the chosen action when in default mode
		[MMFEnumCondition("Mode", (int)Modes.Default)]
		public Actions BlendAction = Actions.Blend;
		/// the duration of the blend, in seconds when in override mode
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float BlendDuration = 1f;
		/// the curve to apply to the blend
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));
		/// the weight to blend from
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float InitialWeight = 0f;
		/// the weight to blend to
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float FinalWeight = 1f;        

		/// <summary>
		/// On custom play, triggers a blend on the target blender, overriding its settings if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (TargetAutoBlend == null)
			{
				Debug.LogWarning(Owner.name + " : this MMFeedbackGlobalPPVolumeAutoBlend needs a TargetAutoBlend, please set one in its inspector.");
				return;
			}
			if (Mode == Modes.Default)
			{
				if (BlendAction == Actions.Blend)
				{
					TargetAutoBlend.Blend();
					return;
				}
				if (BlendAction == Actions.BlendBack)
				{
					TargetAutoBlend.BlendBack();
					return;
				}
			}
			else
			{
				TargetAutoBlend.TimeScale = (ComputedTimescaleMode == TimescaleModes.Scaled) ? MMGlobalPostProcessingVolumeAutoBlend_URP.TimeScales.Scaled : MMGlobalPostProcessingVolumeAutoBlend_URP.TimeScales.Unscaled;
				TargetAutoBlend.BlendDuration = FeedbackDuration;
				TargetAutoBlend.Curve = BlendCurve;
				TargetAutoBlend.InitialWeight = InitialWeight;
				TargetAutoBlend.FinalWeight = FinalWeight;
				TargetAutoBlend.Blend();
			}
            
		}
        
		/// <summary>
		/// On stop we stop our transition
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
            
			if (TargetAutoBlend != null)
			{
				TargetAutoBlend.StopBlending();
			}
		}

		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetAutoBlend.RestoreInitialValues();
		}
	}
}