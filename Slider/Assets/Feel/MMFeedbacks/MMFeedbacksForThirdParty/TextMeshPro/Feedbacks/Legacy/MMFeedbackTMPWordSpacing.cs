using MoreMountains.Tools;
using UnityEngine;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the word spacing of a target TMP over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the word spacing of a target TMP over time.")]
	[FeedbackPath("TextMesh Pro/TMP Word Spacing")]
	public class MMFeedbackTMPWordSpacing : MMFeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		#endif

		#if MM_TEXTMESHPRO
		[Header("Target")]
		/// the TMP_Text component to control
		[Tooltip("the TMP_Text component to control")]
		public TMP_Text TargetTMPText;
		#endif

		[Header("Word Spacing")]
		/// the curve to tween on
		[Tooltip("the curve to tween on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType WordSpacingCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapOne = 10f;
		/// the value to move to in instant mode
		[Tooltip("the value to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
		public float InstantFontSize;
        
		protected override void FillTargets()
		{
			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}
			MMFeedbackBaseTarget target = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = TargetTMPText.gameObject;
			receiver.TargetComponent = TargetTMPText;
			receiver.TargetPropertyName = "wordSpacing";
			receiver.RelativeValue = RelativeValues;
			target.Target = receiver;
			target.LevelCurve = WordSpacingCurve;
			target.RemapLevelZero = RemapZero;
			target.RemapLevelOne = RemapOne;
			target.InstantLevel = InstantFontSize;

			_targets.Add(target);
			#endif
		}

	}
}