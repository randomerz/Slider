using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerTrackEventTypes
	{
		MuteTrack,
		UnmuteTrack,
		SetVolumeTrack,
		PlayTrack,
		PauseTrack,
		StopTrack,
		FreeTrack
	}
    
	/// <summary>
	/// This feedback will let you mute, unmute, play, pause, stop, free or set the volume of a selected track
	///
	/// Example :  MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PauseTrack,MMSoundManager.MMSoundManagerTracks.UI);
	/// will pause the entire UI track
	/// </summary>
	public struct MMSoundManagerTrackEvent
	{
		/// the order to pass to the track
		public MMSoundManagerTrackEventTypes TrackEventType;
		/// the track to pass the order to
		public MMSoundManager.MMSoundManagerTracks Track;
		/// if in SetVolume mode, the volume to which to set the track to
		public float Volume;
        
		public MMSoundManagerTrackEvent(MMSoundManagerTrackEventTypes trackEventType, MMSoundManager.MMSoundManagerTracks track = MMSoundManager.MMSoundManagerTracks.Master, float volume = 1f)
		{
			TrackEventType = trackEventType;
			Track = track;
			Volume = volume;
		}

		static MMSoundManagerTrackEvent e;
		public static void Trigger(MMSoundManagerTrackEventTypes trackEventType, MMSoundManager.MMSoundManagerTracks track = MMSoundManager.MMSoundManagerTracks.Master, float volume = 1f)
		{
			e.TrackEventType = trackEventType;
			e.Track = track;
			e.Volume = volume;
			MMEventManager.TriggerEvent(e);
		}
	}
}