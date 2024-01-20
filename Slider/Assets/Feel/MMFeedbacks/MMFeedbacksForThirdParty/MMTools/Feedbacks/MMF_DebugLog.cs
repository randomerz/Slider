using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you output a message to the console, using a custom MM debug method, or Log, Assertion, Error or Warning logs.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you output a message to the console, using a custom MM debug method, or Log, Assertion, Error or Warning logs.")]
	[FeedbackPath("Debug/Log")]
	public class MMF_DebugLog : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the duration of this feedback is 0
		public override float FeedbackDuration { get { return 0f; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
        
		/// the possible debug modes
		public enum DebugLogModes { DebugLogTime, Log, Assertion, Error, Warning }

		[MMFInspectorGroup("Debug", true, 17)]
		/// the selected debug mode
		[Tooltip("the selected debug mode")]
		public DebugLogModes DebugLogMode = DebugLogModes.DebugLogTime;

		/// the message to display 
		[Tooltip("the message to display")]
		[TextArea] 
		public string DebugMessage;
		/// the color of the message when in DebugLogTime mode
		[Tooltip("the color of the message when in DebugLogTime mode")]
		[MMFEnumCondition("DebugLogMode", (int) DebugLogModes.DebugLogTime)]
		public Color DebugColor = Color.cyan;
		/// whether or not to display the frame count when in DebugLogTime mode
		[Tooltip("whether or not to display the frame count when in DebugLogTime mode")]
		[MMFEnumCondition("DebugLogMode", (int) DebugLogModes.DebugLogTime)]
		public bool DisplayFrameCount = true;

		/// <summary>
		/// On Play we output our message to the console using the selected mode
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (DebugLogMode)
			{
				case DebugLogModes.Assertion:
					Debug.LogAssertion(DebugMessage);
					break;
				case DebugLogModes.Log:
					Debug.Log(DebugMessage);
					break;
				case DebugLogModes.Error:
					Debug.LogError(DebugMessage);
					break;
				case DebugLogModes.Warning:
					Debug.LogWarning(DebugMessage);
					break;
				case DebugLogModes.DebugLogTime:
					string color = "#" + ColorUtility.ToHtmlStringRGB(DebugColor);
					MMDebug.DebugLogTime(DebugMessage, color, 3, DisplayFrameCount);
					break;
			}
		}
	}
}