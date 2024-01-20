using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you pilot a Global PostProcessing Volume AutoBlend component. A GPPVAB component is placed on a PostProcessing Volume, and will let you control and blend its weight over time on demand.    
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you pilot a Global PostProcessing Volume AutoBlend component. " +
	              "A GPPVAB component is placed on a PostProcessing Volume, and will let you control and blend its weight over time on demand.")]
	[FeedbackPath("PostProcess/Global PP Volume Auto Blend")]
	public class MMFeedbackGlobalPPVolumeAutoBlend : MMFeedback
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
		#endif
		/// defines the duration of the feedback
		public override float FeedbackDuration
		{
			get
			{
				if (Mode == Modes.Override)
				{
					return BlendDuration;
				}
				else
				{
					if (TargetAutoBlend == null)
					{
						return 0.1f;
					}
					else
					{
						return TargetAutoBlend.BlendDuration;
					}
				}
			}
		}
               
		[Header("PostProcess Volume Blend")]
		/// the target auto blend to pilot with this feedback
		[Tooltip("the target auto blend to pilot with this feedback")]
		public MMGlobalPostProcessingVolumeAutoBlend TargetAutoBlend;
		/// the chosen mode
		[Tooltip("the chosen mode")]
		public Modes Mode = Modes.Default;
		/// the chosen action when in default mode
		[Tooltip("the chosen action when in default mode")]
		[MMFEnumCondition("Mode", (int)Modes.Default)]
		public Actions BlendAction = Actions.Blend;
		/// the duration of the blend, in seconds when in override mode
		[Tooltip("the duration of the blend, in seconds when in override mode")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float BlendDuration = 1f;
		/// the curve to apply to the blend
		[Tooltip("the curve to apply to the blend")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));
		/// the weight to blend from
		[Tooltip("the weight to blend from")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float InitialWeight = 0f;
		/// the weight to blend to
		[Tooltip("the weight to blend to")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float FinalWeight = 1f;
		/// whether or not to reset to the initial value at the end of the shake
		[Tooltip("whether or not to reset to the initial value at the end of the shake")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public bool ResetToInitialValueOnEnd = true;

		/// <summary>
		/// On custom play, triggers a blend on the target blender, overriding its settings if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
        
			#if MM_POSTPROCESSING
            
			if (TargetAutoBlend == null)
			{
				Debug.LogWarning(this.name + " : this MMFeedbackGlobalPPVolumeAutoBlend needs a TargetAutoBlend, please set one in its inspector.");
				return;
			}
			if (Mode == Modes.Default)
			{
				if (!NormalPlayDirection)
				{
					if (BlendAction == Actions.Blend)
					{
						TargetAutoBlend.BlendBack();
						return;
					}
					if (BlendAction == Actions.BlendBack)
					{
						TargetAutoBlend.Blend();
						return;
					}
				}
				else
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
			}
			else
			{
				TargetAutoBlend.BlendDuration = BlendDuration;
				TargetAutoBlend.Curve = BlendCurve;
				if (!NormalPlayDirection)
				{
					TargetAutoBlend.InitialWeight = FinalWeight;
					TargetAutoBlend.FinalWeight = InitialWeight;   
				}
				else
				{
					TargetAutoBlend.InitialWeight = InitialWeight;
					TargetAutoBlend.FinalWeight = FinalWeight;    
				}
				TargetAutoBlend.ResetToInitialValueOnEnd = ResetToInitialValueOnEnd;
				TargetAutoBlend.Blend();
			}
			#endif
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
        
			#if MM_POSTPROCESSING
            
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			if (TargetAutoBlend != null)
			{
				TargetAutoBlend.StopBlending();
			}
			#endif
		}
	}
}