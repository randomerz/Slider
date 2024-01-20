using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A class handling the lifecycle of the balls included in the MMFeedbacks demo
	/// It waits for 2 seconds after the spawn of the ball, and destroys it, playing a MMFeedbacks while it does so
	/// </summary>
	public class DemoBall : MonoBehaviour
	{
		/// the duration (in seconds) of the life of the ball
		public float LifeSpan = 2f;
		/// the feedback to play when the ball dies
		public MMFeedbacks DeathFeedback;


		/// <summary>
		/// On start, we trigger the programmed death of the ball
		/// </summary>
		protected virtual void Start()
		{
			StartCoroutine(ProgrammedDeath());
		}

		/// <summary>
		/// Waits for 2 seconds, then kills the ball object after having played the MMFeedbacks
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ProgrammedDeath()
		{
			yield return MMCoroutine.WaitFor(LifeSpan);
			DeathFeedback?.PlayFeedbacks();
			this.gameObject.SetActive(false);
		}
	}
}