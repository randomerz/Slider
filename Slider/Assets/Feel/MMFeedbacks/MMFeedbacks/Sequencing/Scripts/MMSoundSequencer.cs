using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A MMSequencer with ready made slots to play sounds
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Sequencing/MMSoundSequencer")]
	public class MMSoundSequencer : MMSequencer
	{
		/// the list of audio clips to play (one per track)
		[Tooltip("the list of audio clips to play (one per track)")]
		public List<AudioClip> Sounds;

		protected List<AudioSource> _audioSources;
        
		/// <summary>
		/// On Initialization we create our audiosources to play later
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_audioSources = new List<AudioSource>();
			foreach(AudioClip sound in Sounds)
			{
				GameObject asGO = new GameObject();
				SceneManager.MoveGameObjectToScene(asGO, this.gameObject.scene);
				asGO.name = "AudioSource - " + sound.name;
				asGO.transform.SetParent(this.transform);
				AudioSource source = asGO.AddComponent<AudioSource>();
				source.loop = false;
				source.playOnAwake = false;
				source.clip = sound;
				source.volume = 1f;
				source.pitch = 1f;
				_audioSources.Add(source);
			}
		}

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
					if ((_audioSources.Count > i) && (_audioSources[i] != null))
					{
						_audioSources[i].Play();
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
			_audioSources[index].Play();
		}

		/// <summary>
		/// When looking for changes we make sure we have enough sounds in our array
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
			if (Sounds.Count < Sequence.SequenceTracks.Count)
			{
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					if (i >= Sounds.Count)
					{
						Sounds.Add(null);
					}
				}
			}
			if (Sounds.Count > Sequence.SequenceTracks.Count)
			{
				Sounds.Clear();
				for (int i = 0; i < Sequence.SequenceTracks.Count; i++)
				{
					Sounds.Add(null);
				}
			}
		}
	}    
}