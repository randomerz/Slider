using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to chain any number of target MMF Players and play them in sequence, with optional delays before and after
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to chain any number of target MMF Players and play them in sequence, with optional delays before and after")]
	[FeedbackPath("Feedbacks/MMF Player Chain")]
	public class MMF_PlayerChain : MMF_Feedback
	{
		/// <summary>
		/// A class used to store and define items in a chain of MMF Players
		/// </summary>
		[Serializable]
		public class PlayerChainItem
		{
			/// the target MMF Player 
			[Tooltip("the target MMF Player")]
			public MMF_Player TargetPlayer;
			/// a delay in seconds to wait for before playing this MMF Player (x) and after (y)
			[Tooltip("a delay in seconds to wait for before playing this MMF Player (x) and after (y)")]
			[MMVector("Before", "After")]
			public Vector2 Delay;
			/// whether this player is active in the list or not. Inactive players will be skipped when playing the chain of players
			[Tooltip("whether this player is active in the list or not. Inactive players will be skipped when playing the chain of players")]
			public bool Inactive = false;
		}
		
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		#endif
		/// the duration of this feedback is the duration of the chain
		public override float FeedbackDuration 
		{
			get
			{
				if ((Players == null) || (Players.Count == 0))
				{
					return 0f;
				}
				
				float totalDuration = 0f;
				foreach (PlayerChainItem item in Players)
				{
					if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
					{
						continue;
					}

					totalDuration += item.Delay.x;
					totalDuration += item.TargetPlayer.TotalDuration;
					totalDuration += item.Delay.y; 
				}
				return totalDuration;
			} 
		}

		[MMFInspectorGroup("Feedbacks", true, 79)]
		/// the list of MMF Player that make up the chain. The chain's items will be played from index 0 to the last in the list
		[Tooltip("the list of MMF Player that make up the chain. The chain's items will be played from index 0 to the last in the list")]
		public List<PlayerChainItem> Players;

		/// <summary>
		/// On Play we start our chain
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if ((Players == null) || (Players.Count == 0))
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			Owner.StartCoroutine(PlayChain());
		}

		/// <summary>
		/// Plays all players in the chain in sequence
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PlayChain()
		{
			foreach (PlayerChainItem item in Players)
			{
				if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
				{
					continue;
				}

				if (item.Delay.x > 0) { yield return WaitFor(item.Delay.x); }
				
				item.TargetPlayer.PlayFeedbacks();
				yield return WaitFor(item.TargetPlayer.TotalDuration);
				
				if (item.Delay.y > 0) { yield return WaitFor(item.Delay.y); }
			}
		}

		/// <summary>
		/// On skip to the end, we skip for all players in our chain
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			foreach (PlayerChainItem item in Players)
			{
				if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
				{
					continue;
				}

				item.TargetPlayer.PlayFeedbacks();
				item.TargetPlayer.SkipToTheEnd();
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

			foreach (PlayerChainItem item in Players)
			{
				item.TargetPlayer.RestoreInitialValues();
			}
		}
	}
}