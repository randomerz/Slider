using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the color of a target Graphic over time.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the color of a target Graphic over time.")]
	[FeedbackPath("UI/Graphic")]
	public class MMF_Graphic : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGraphic == null); }
		public override string RequiredTargetText { get { return TargetGraphic != null ? TargetGraphic.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetGraphic be set to be able to work properly. You can set one below."; } }
		#endif

		/// the duration of this feedback is the duration of the Graphic, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGraphic = FindAutomatedTarget<Graphic>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Graphic", true, 54, true)]
		/// the Graphic to affect when playing the feedback
		[Tooltip("the Graphic to affect when playing the feedback")]
		public Graphic TargetGraphic;
		/// whether the feedback should affect the Graphic instantly or over a period of time
		[Tooltip("whether the feedback should affect the Graphic instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the Graphic should change over time
		[Tooltip("how long the Graphic should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not that Graphic should be turned off on start
		[Tooltip("whether or not that Graphic should be turned off on start")]
		public bool StartsOff = false;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("if this is true, the target will be disabled when this feedbacks is stopped")] 
		public bool DisableOnStop = false;
        
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// whether or not to modify the color of the Graphic
		[Tooltip("whether or not to modify the color of the Graphic")]
		public bool ModifyColor = true;
		/// the colors to apply to the Graphic over time
		[Tooltip("the colors to apply to the Graphic over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("the color to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;

		protected Coroutine _coroutine;
		protected Color _initialColor;

		/// <summary>
		/// On init we turn the Graphic off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}
		}

		/// <summary>
		/// On Play we turn our Graphic on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_initialColor = TargetGraphic.color;
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						TargetGraphic.color = InstantColor;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(GraphicSequence());
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the Graphic
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator GraphicSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetGraphicValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetGraphicValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the Graphic on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetGraphicValues(float time)
		{
			if (ModifyColor)
			{
				TargetGraphic.color = ColorOverTime.Evaluate(time);
			}
		}

		/// <summary>
		/// Turns the Graphic off on stop
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			IsPlaying = false;
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && DisableOnStop)
			{
				Turn(false);    
			}
		}

		/// <summary>
		/// Turns the Graphic on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			TargetGraphic.gameObject.SetActive(status);
			TargetGraphic.enabled = status;
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
			TargetGraphic.color = _initialColor;
		}
	}
}