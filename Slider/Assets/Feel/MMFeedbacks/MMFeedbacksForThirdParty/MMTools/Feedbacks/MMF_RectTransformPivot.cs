using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the position of a RectTransform's pivot over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the position of a RectTransform's pivot over time")]
	[FeedbackPath("UI/RectTransform Pivot")]
	public class MMF_RectTransformPivot : MMF_FeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRectTransform == null); }
		public override string RequiredTargetText { get { return TargetRectTransform != null ? TargetRectTransform.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRectTransform be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRectTransform = FindAutomatedTarget<RectTransform>();

		[MMFInspectorGroup("Target RectTransform", true, 37, true)]
		/// the RectTransform whose position you want to control over time 
		[Tooltip("the RectTransform whose position you want to control over time")]
		public RectTransform TargetRectTransform;
        
		[MMFInspectorGroup("Pivot", true, 39)] 
		/// The curve along which to evaluate the position of the RectTransform's pivot
		[Tooltip("The curve along which to evaluate the position of the RectTransform's pivot")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType SpeedCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the position to remap the curve's 0 to 
		[Tooltip("the position to remap the curve's 0 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 RemapZero = Vector2.zero;
		/// the position to remap the curve's 1 to
		[Tooltip("the position to remap the curve's 1 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 RemapOne = Vector2.one;
        
		protected override void FillTargets()
		{
			if (TargetRectTransform == null)
			{
				return;
			}
            
			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = TargetRectTransform.gameObject;
			receiver.TargetComponent = TargetRectTransform;
			receiver.TargetPropertyName = "pivot";
			receiver.RelativeValue = RelativeValues;
			receiver.Vector2RemapZero = RemapZero;
			receiver.Vector2RemapOne = RemapOne;
			target.Target = receiver;
			target.LevelCurve = SpeedCurve;
			target.RemapLevelZero = 0f;
			target.RemapLevelOne = 1f;
			target.InstantLevel = 1f;

			_targets.Add(target);
		}
	}
}