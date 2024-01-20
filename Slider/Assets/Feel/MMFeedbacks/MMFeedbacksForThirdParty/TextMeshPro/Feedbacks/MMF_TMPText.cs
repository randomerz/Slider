using UnityEngine;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the text of a target TMP text component
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the text of a target TMP text component")]
	#if MM_TEXTMESHPRO
	[FeedbackPath("TextMesh Pro/TMP Text")]
	#endif
	public class MMF_TMPText : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTMPText be set to be able to work properly. You can set one below."; } }
		#endif
		#if UNITY_EDITOR && MM_TEXTMESHPRO
		public override bool EvaluateRequiresSetup() { return (TargetTMPText == null); }
		public override string RequiredTargetText { get { return TargetTMPText != null ? TargetTMPText.name : "";  } }
		#endif
        
		#if MM_TEXTMESHPRO
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTMPText = FindAutomatedTarget<TMP_Text>();

		[MMFInspectorGroup("TextMeshPro Change Text", true, 12, true)]
		/// the target TMP_Text component we want to change the text on
		[Tooltip("the target TMP_Text component we want to change the text on")]
		public TMP_Text TargetTMPText;
		/// the new text to replace the old one with
		[Tooltip("the new text to replace the old one with")]
		[TextArea]
		public string NewText = "Hello World";
		#endif

		protected string _initialText;
        
		/// <summary>
		/// On play we change the text of our target TMPText
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			#if MM_TEXTMESHPRO
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (TargetTMPText == null)
			{
				return;
			}

			_initialText = TargetTMPText.text;
			TargetTMPText.text = NewText;
			#endif
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
			#if MM_TEXTMESHPRO
			TargetTMPText.text = _initialText;
			#endif
		}
	}
}