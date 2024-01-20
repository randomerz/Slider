using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// the possible states for sequence notes
	public enum MMSequenceTrackStates { Idle, Down, Up }

	/// <summary>
	/// A class describing the contents of a sequence note, basically a timestamp and the ID to play at that timestamp
	/// </summary>
	[System.Serializable]
	public class MMSequenceNote
	{
		public float Timestamp;
		public int ID;

		public virtual MMSequenceNote Copy()
		{
			MMSequenceNote newNote = new MMSequenceNote();
			newNote.ID = this.ID;
			newNote.Timestamp = this.Timestamp;
			return newNote;
		}
	}

	/// <summary>
	/// A class describing the properties of a sequence's track : ID, color (for the inspector), Key (for the recorder), State (for the recorder)
	/// </summary>
	[System.Serializable]
	public class MMSequenceTrack
	{
		public int ID = 0;
		public Color TrackColor;
		public KeyCode Key = KeyCode.Space;
		public bool Active = true;
		[MMFReadOnly]
		public MMSequenceTrackStates State = MMSequenceTrackStates.Idle;
		[HideInInspector]
		public bool Initialized = false;
        
		public virtual void SetDefaults(int index)
		{
			if (!Initialized)
			{
				ID = index;
				TrackColor = MMSequence.RandomSequenceColor();
				Key = KeyCode.Space;
				Active = true;
				State = MMSequenceTrackStates.Idle;
				Initialized = true;
			}            
		}
	}

	/// <summary>
	/// A class used to store sequence notes
	/// </summary>
	[System.Serializable]
	public class MMSequenceList
	{
		public List<MMSequenceNote> Line;
	}

	/// <summary>
	/// This scriptable object holds "sequences", data used to record and play events in sequence
	/// MMSequences can be played by MMFeedbacks from their Timing section, by Sequencers and potentially other classes
	/// </summary>
	[CreateAssetMenu(menuName = "MoreMountains/Sequencer/MMSequence")]
	public class MMSequence : ScriptableObject
	{
		[Header("Sequence")]
		/// the length (in seconds) of the sequence
		[Tooltip("the length (in seconds) of the sequence")]
		[MMFReadOnly]
		public float Length;
		/// the original sequence (as outputted by the input sequence recorder)
		[Tooltip("the original sequence (as outputted by the input sequence recorder)")]
		public MMSequenceList OriginalSequence;
		/// the duration in seconds to apply after the last input
		[Tooltip("the duration in seconds to apply after the last input")]
		public float EndSilenceDuration = 0f;

		[Header("Sequence Contents")]
		/// the list of tracks for this sequence
		[Tooltip("the list of tracks for this sequence")]
		public List<MMSequenceTrack> SequenceTracks;

		[Header("Quantizing")]
		/// whether this sequence should be used in quantized form or not
		[Tooltip("whether this sequence should be used in quantized form or not")]
		public bool Quantized;
		/// the target BPM for this sequence
		[Tooltip("the target BPM for this sequence")]
		public int TargetBPM = 120;
		/// the contents of the quantized sequence
		[Tooltip("the contents of the quantized sequence")]
		public List<MMSequenceList> QuantizedSequence;
        
		[Space]
		[Header("Controls")]
		[MMFInspectorButton("RandomizeTrackColors")]
		public bool RandomizeTrackColorsButton;
        
		protected float[] _quantizedBeats; 
		protected List<MMSequenceNote> _deleteList;

		/// <summary>
		/// Compares and sorts two sequence notes
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		static int SortByTimestamp(MMSequenceNote p1, MMSequenceNote p2)
		{
			return p1.Timestamp.CompareTo(p2.Timestamp);
		}

		/// <summary>
		/// Sorts the original sequence based on timestamps
		/// </summary>
		public virtual void SortOriginalSequence()
		{
			OriginalSequence.Line.Sort(SortByTimestamp);
		}

		/// <summary>
		/// Quantizes the original sequence, filling the QuantizedSequence list, arranging events on the beat
		/// </summary>
		public virtual void QuantizeOriginalSequence()
		{
			ComputeLength();
			QuantizeSequenceToBPM(OriginalSequence.Line);
		}

		/// <summary>
		/// Computes the length of the sequence
		/// </summary>
		public virtual void ComputeLength()
		{
			Length = OriginalSequence.Line[OriginalSequence.Line.Count - 1].Timestamp + EndSilenceDuration;
		}

		/// <summary>
		/// Makes every timestamp in the sequence match the BPM track
		/// </summary>
		public virtual void QuantizeSequenceToBPM(List<MMSequenceNote> baseSequence)
		{
			float sequenceLength = Length;
			float beatDuration = 60f / TargetBPM;
			int numberOfBeatsInSequence = (int)(sequenceLength / beatDuration);
			QuantizedSequence = new List<MMSequenceList>();
			_deleteList = new List<MMSequenceNote>();
			_deleteList.Clear();

			// we fill the BPM track with the computed timestamps
			_quantizedBeats = new float[numberOfBeatsInSequence];
			for (int i = 0; i < numberOfBeatsInSequence; i++)
			{
				_quantizedBeats[i] = i * beatDuration;
			}
            
			for (int i = 0; i < SequenceTracks.Count; i++)
			{
				QuantizedSequence.Add(new MMSequenceList());
				QuantizedSequence[i].Line = new List<MMSequenceNote>();
				for (int j = 0; j < numberOfBeatsInSequence; j++)
				{
					MMSequenceNote newNote = new MMSequenceNote();
					newNote.ID = -1;
					newNote.Timestamp = _quantizedBeats[j];
					QuantizedSequence[i].Line.Add(newNote);

					foreach (MMSequenceNote note in baseSequence)
					{
						float newTimestamp = RoundFloatToArray(note.Timestamp, _quantizedBeats);
						if ((newTimestamp == _quantizedBeats[j]) && (note.ID == SequenceTracks[i].ID))
						{
							QuantizedSequence[i].Line[j].ID = note.ID;
						}
					}
				}
			}        
		}

		/// <summary>
		/// On validate, we initialize our track's properties
		/// </summary>
		protected virtual void OnValidate()
		{
			for (int i = 0; i < SequenceTracks.Count; i++)
			{
				SequenceTracks[i].SetDefaults(i);
			}
		}

		/// <summary>
		/// Randomizes track colors
		/// </summary>
		protected virtual void RandomizeTrackColors()
		{
			foreach(MMSequenceTrack track in SequenceTracks)
			{
				track.TrackColor = RandomSequenceColor();
			}
		}

		/// <summary>
		/// Returns a random color for the sequence tracks
		/// </summary>
		/// <returns></returns>
		public static Color RandomSequenceColor()
		{
			int random = UnityEngine.Random.Range(0, 32);
			switch (random)
			{
				case 0: return new Color32(240, 248, 255, 255); 
				case 1: return new Color32(127, 255, 212, 255);
				case 2: return new Color32(245, 245, 220, 255);
				case 3: return new Color32(95, 158, 160, 255);
				case 4: return new Color32(255, 127, 80, 255);
				case 5: return new Color32(0, 255, 255, 255);
				case 6: return new Color32(255, 215, 0, 255);
				case 7: return new Color32(255, 0, 255, 255);
				case 8: return new Color32(50, 128, 120, 255);
				case 9: return new Color32(173, 255, 47, 255);
				case 10: return new Color32(255, 105, 180, 255);
				case 11: return new Color32(75, 0, 130, 255);
				case 12: return new Color32(255, 255, 240, 255);
				case 13: return new Color32(124, 252, 0, 255);
				case 14: return new Color32(255, 160, 122, 255);
				case 15: return new Color32(0, 255, 0, 255);
				case 16: return new Color32(245, 255, 250, 255);
				case 17: return new Color32(255, 228, 225, 255);
				case 18: return new Color32(218, 112, 214, 255);
				case 19: return new Color32(255, 192, 203, 255);
				case 20: return new Color32(255, 0, 0, 255);
				case 21: return new Color32(196, 112, 255, 255);
				case 22: return new Color32(250, 128, 114, 255);
				case 23: return new Color32(46, 139, 87, 255);
				case 24: return new Color32(192, 192, 192, 255);
				case 25: return new Color32(135, 206, 235, 255);
				case 26: return new Color32(0, 255, 127, 255);
				case 27: return new Color32(210, 180, 140, 255);
				case 28: return new Color32(0, 128, 128, 255);
				case 29: return new Color32(255, 99, 71, 255);
				case 30: return new Color32(64, 224, 208, 255);
				case 31: return new Color32(255, 255, 0, 255);
				case 32: return new Color32(154, 205, 50, 255);
			}
			return new Color32(240, 248, 255, 255); 
		}
        
		/// <summary>
		/// Rounds a float to the closest float in an array (array has to be sorted)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="array"></param>
		/// <returns></returns>
		public static float RoundFloatToArray(float value, float[] array)
		{
			int min = 0;
			if (array[min] >= value) return array[min];

			int max = array.Length - 1;
			if (array[max] <= value) return array[max];

			while (max - min > 1)
			{
				int mid = (max + min) / 2;

				if (array[mid] == value)
				{
					return array[mid];
				}
				else if (array[mid] < value)
				{
					min = mid;
				}
				else
				{
					max = mid;
				}
			}

			if (array[max] - value <= value - array[min])
			{
				return array[max];
			}
			else
			{
				return array[min];
			}
		}
	}
}