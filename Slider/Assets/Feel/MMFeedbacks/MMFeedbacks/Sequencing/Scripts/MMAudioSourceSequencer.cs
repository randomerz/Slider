using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A MMSequencer with ready made slots to play AudioSources
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMAudioSourceSequencer")]
	public class MMAudioSourceSequencer : MMSequencer
	{
		/// the list of audio sources to play (one per track)
		[Tooltip("the list of audio sources to play (one per track)")]
		public List<AudioSource> AudioSources;
                
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
					if ((AudioSources.Count > i) && (AudioSources[i] != null))
					{
						AudioSources[i].Play();
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
			AudioSources[index].Play();
		}

		/// <summary>
		/// When looking for changes we make sure we have enough audiosources in our array
		/// </summary>
		public override void EditorMaintenance()
		{
			base.EditorMaintenance();
			SetupSounds();
		}

		/// <summary>
		/// Ensures the array is always the right length
		/// </summary>
		public virtual void SetupSounds()
		{
			if (Sequence == null)
			{
				return;
			}
			// setup events
			if (AudioSources.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= AudioSources.Count)
					{
						AudioSources.Add(null);
					}
				}
			}
			if (AudioSources.Count > Sequence.SequenceTracks.Count)
			{
				AudioSources.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					AudioSources.Add(null);
				}
			}
		}
	}    
}