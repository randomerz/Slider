using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A definition of a song, a part of a MMSM Playlist
	/// </summary>
	[Serializable]
	public class MMSMPlaylistSong
	{
		/// the name of the song, used only for organizational purposes in the inspector
		[Tooltip("the name of the song, used only for organizational purposes in the inspector")]
		public string Name;
		/// the clip to play when this song plays
		[Tooltip("the clip to play when this song plays")]
		public AudioClip Clip;
		/// the amount of time this song's been played
		[Tooltip("the amount of time this song's been played")]
		[MMReadOnly] 
		public int PlayCount;
		/// the many options to control this song
		[Tooltip("the many options to control this song")]
		public MMSoundManagerPlayOptions Options;

		/// <summary>
		/// On init, we reset our play count
		/// </summary>
		public virtual void Initialization()
		{
			PlayCount = 0;
		}
	}
	
	[CreateAssetMenu(menuName = "MoreMountains/Audio/MMSM Playlist")]
	[Serializable]
	public class MMSMPlaylist : ScriptableObject
	{
		public enum PlayModes { PlayForever, PlayOnce, PlayXTimes }
		public enum PlayOrders { Normal, ReverseOrder, Random, RandomUnique }
		
		[Header("Play Modes")]
		/// the sound manager track on which to play this playlist's songs
		[Tooltip("the sound manager track on which to play this playlist's songs")]
		public MMSoundManager.MMSoundManagerTracks Track = MMSoundManager.MMSoundManagerTracks.Music;
		/// the order in which to play songs (top to bottom, bottom to top, random, or random while trying to maintain playcount across songs
		[Tooltip("the order in which to play songs (top to bottom, bottom to top, random, or random while trying to maintain playcount across songs")]
		public PlayOrders PlayOrder = PlayOrders.Normal;
		/// if this is true, random seed will be randomized by the system clock
		[Tooltip("if this is true, random seed will be randomized by the system clock")]
		[MMEnumCondition("PlayOrder", (int)PlayOrders.Random, (int)PlayOrders.RandomUnique)]
		public bool RandomizeOrderSeed = true;
		/// whether to play this playlist forever, only once, or play songs until total playcount reaches MaxAmountOfPlays
		[Tooltip("whether to play this playlist forever, only once, or play songs until total playcount reaches MaxAmountOfPlays")]
		public PlayModes PlayMode = PlayModes.PlayForever;
		/// when in PlayXTimes mode, the max amount of plays before this playlist ends
		[Tooltip("when in PlayXTimes mode, the max amount of plays before this playlist ends")]
		[MMEnumCondition("PlayMode", (int)PlayModes.PlayXTimes)]
		public int MaxAmountOfPlays = 10;
		/// a playlist to switch to when reaching the end of this playlist
		[Tooltip("a playlist to switch to when reaching the end of this playlist")]
		[MMEnumCondition("PlayMode",(int)PlayModes.PlayOnce, (int)PlayModes.PlayXTimes)]
		public MMSMPlaylist NextPlaylist;
		/// the list of songs to play on this playlist
		[Tooltip("the list of songs to play on this playlist")]
		public List<MMSMPlaylistSong> Songs;
		
		[Header("Debug")]
		/// the total number of times songs in this playlist have been played 
		[Tooltip("the total number of times songs in this playlist have been played ")]
		[MMReadOnly] 
		public int PlayCount;

		protected List<int> _randomUniqueCandidates;

		/// <summary>
		/// On init, we initialize all our songs
		/// </summary>
		public virtual void Initialization()
		{
			PlayCount = 0;
			_randomUniqueCandidates = new List<int>();
			foreach (MMSMPlaylistSong song in Songs)
			{
				song.Initialization();
			}
		}
		
		/// <summary>
		/// Picks the index of the next song to play, returns the index of the song, or -2 if the end of the
		/// playlist's been reached, and -1 if the player should go idle
		/// </summary>
		/// <param name="direction"></param>
		/// <returns>
		/// -2 : end of playlist
		/// -1 : go to idle
		/// 0+ : next index to play in the playlist
		/// </returns>
		public virtual int PickNextIndex(int direction, int currentSongIndex, ref int queuedSongIndex)
		{
			int newIndex = currentSongIndex;
			
			if (Songs.Count == 0)
			{
				return -1;
			}

			if (queuedSongIndex != -1)
			{
				int newRequestedIndex = queuedSongIndex;
				queuedSongIndex = -1;
				return newRequestedIndex;
			}
			
			if ((PlayCount >= Songs.Count) && (PlayMode == PlayModes.PlayOnce))
			{
				return -2;
			}

			if ((PlayMode == PlayModes.PlayXTimes) && (PlayCount >= MaxAmountOfPlays))
			{
				return -2;
			}

			switch (PlayOrder)
			{
				case PlayOrders.Random:
					while (newIndex == currentSongIndex)
					{
						newIndex = Random.Range(0, Songs.Count);
					}
					return newIndex;
				
				case PlayOrders.RandomUnique:
					
					bool allPlayed = true;
					int lowestPlayCount = int.MaxValue;
					_randomUniqueCandidates.Clear();
					
					for (int i = 0; i < Songs.Count; i++)
					{
						if (Songs[i].PlayCount <= lowestPlayCount && i != currentSongIndex)
						{
							allPlayed = false;
							lowestPlayCount = Songs[i].PlayCount;
							_randomUniqueCandidates.Add(i);	
						}
					}
					
					if (allPlayed)
					{
						while (newIndex == currentSongIndex)
						{
							newIndex = Random.Range(0, Songs.Count);
						}
					}
					else
					{
						int random = Random.Range(0, _randomUniqueCandidates.Count);
						
						newIndex = _randomUniqueCandidates[random];
					}

					return newIndex;
				
				case PlayOrders.Normal:
					break;
				
				case PlayOrders.ReverseOrder:
					direction = -1;
					break;
			}
			
			if (direction > 0)
			{
				newIndex = (currentSongIndex + 1) % Songs.Count;
			}
			else
			{
				newIndex = (currentSongIndex - 1);
				if (newIndex < 0)
				{
					newIndex = Songs.Count - 1;
				}
			}

			return newIndex;
		}

		/// <summary>
		/// Resets the playlist's play count and the playcount of all songs
		/// </summary>
		public virtual void ResetPlayCount()
		{
			PlayCount = 0;
			foreach (MMSMPlaylistSong song in Songs)
			{
				song.PlayCount = 0;
			}
		}
		
		/// <summary>
		/// On Validate we initialize our options
		/// </summary>
		protected virtual void OnValidate()
		{
			foreach (MMSMPlaylistSong song in Songs)
			{
				if (!song.Options.Initialized)
				{
					song.Options = MMSoundManagerPlayOptions.Default;
				}
			}
		}
	}
}