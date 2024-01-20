using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to control one or more target MMF Players
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to control one or more target MMF Players")]
	[FeedbackPath("Feedbacks/MMF Player Control")]
	public class MMF_PlayerControl : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => Mode.ToString();
		#endif
		
		public override bool HasChannel => false;

		public enum Modes
		{
			PlayFeedbacks,
			StopFeedbacks,
			PauseFeedbacks,
			ResumeFeedbacks,
			Initialization,
			PlayFeedbacksInReverse,
			PlayFeedbacksOnlyIfReversed,
			PlayFeedbacksOnlyIfNormalDirection,
			ResetFeedbacks,
			Revert,
			SetDirectionTopToBottom,
			SetDirectionBottomToTop,
			RestoreInitialValues,
			SkipToTheEnd,
			RefreshCache
		}
	
        
		[MMFInspectorGroup("MMF Player", true, 79)]
        
		/// a list of target MMF_Players to play
		[Tooltip("a specific MMFeedbacks / MMF_Player to play")]
		public List<MMF_Player> TargetPlayers;

		public Modes Mode = Modes.PlayFeedbacks;

		/// <summary>
		/// On init we turn the light off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
		}

		/// <summary>
		/// On Play we trigger the selected method on our target players
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (TargetPlayers.Count == 0)
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.PlayFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacks(position, feedbacksIntensity); }
					break;
				case Modes.StopFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.StopFeedbacks(); }
					break;
				case Modes.PauseFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.PauseFeedbacks(); }
					break;
				case Modes.ResumeFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.ResumeFeedbacks(); }
					break;
				case Modes.Initialization:
					foreach (MMF_Player player in TargetPlayers) { player.Initialization(); }
					break;
				case Modes.PlayFeedbacksInReverse:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacksInReverse(position, feedbacksIntensity); }
					break;
				case Modes.PlayFeedbacksOnlyIfReversed:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacksOnlyIfReversed(position, feedbacksIntensity); }
					break;
				case Modes.PlayFeedbacksOnlyIfNormalDirection:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacksOnlyIfNormalDirection(position, feedbacksIntensity); }
					break;
				case Modes.ResetFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.ResetFeedbacks(); }
					break;
				case Modes.Revert:
					foreach (MMF_Player player in TargetPlayers) { player.Revert(); }
					break;
				case Modes.SetDirectionTopToBottom:
					foreach (MMF_Player player in TargetPlayers) { player.SetDirectionTopToBottom(); }
					break;
				case Modes.SetDirectionBottomToTop:
					foreach (MMF_Player player in TargetPlayers) { player.SetDirectionBottomToTop(); }
					break;
				case Modes.RestoreInitialValues:
					foreach (MMF_Player player in TargetPlayers) { player.RestoreInitialValues(); }
					break;
				case Modes.SkipToTheEnd:
					foreach (MMF_Player player in TargetPlayers) { player.SkipToTheEnd(); }
					break;
				case Modes.RefreshCache:
					foreach (MMF_Player player in TargetPlayers) { player.RefreshCache(); }
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}