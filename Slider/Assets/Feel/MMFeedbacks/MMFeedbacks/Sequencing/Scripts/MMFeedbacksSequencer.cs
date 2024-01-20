using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A MMSequencer with ready made slots to play MMFeedbacks
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMFeedbacksSequencer")]
	public class MMFeedbacksSequencer : MMSequencer
	{
		/// the list of audio clips to play (one per track)
		[Tooltip("the list of audio clips to play (one per track)")]
		public List<MMFeedbacks> Feedbacks;

		/// <summary>
		/// On beat we play our audio sources
		/// </summary>
		protected override void OnBeat()
		{
			base.OnBeat();
			for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
			{
				if ((Sequence.SequenceTracks[i].Active) && (Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID != -1))
				{
					if ((Feedbacks.Count > i) && (Feedbacks[i] != null))
					{
						Feedbacks[i].PlayFeedbacks();
					}
				}
			}
		}

		/// <summary>
		/// When playing our event for control, we play our audiosource
		/// </summary>
		/// <param name="index"></param>
		public override void PlayTrackEvent(int index)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			base.PlayTrackEvent(index);
			Feedbacks[index].PlayFeedbacks();
		}

		/// <summary>
		/// When looking for changes we make sure we have enough sounds in our array
		/// </summary>
		public override void EditorMaintenance()
		{
			base.EditorMaintenance();
			SetupFeedbacks();
		}

		/// <summary>
		/// Ensures the array is always the right length
		/// </summary>
		public virtual void SetupFeedbacks()
		{
			if (Sequence == null)
			{
				return;
			}
			// setup events
			if (Feedbacks.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= Feedbacks.Count)
					{
						Feedbacks.Add(null);
					}
				}
			}
			if (Feedbacks.Count > Sequence.SequenceTracks.Count)
			{
				Feedbacks.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					Feedbacks.Add(null);
				}
			}
		}
	}
}