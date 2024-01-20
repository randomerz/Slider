using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A class used to expose a beat level from a target MMAudioAnalyzer, to be broadcasted by a MMAudioBroadcaster
	/// </summary>
	public class MMRadioSignalAudioAnalyzer : MMRadioSignal
	{
		[Header("Audio Analyzer")]
		/// the MMAudioAnalyzer to read the value on
		public MMAudioAnalyzer TargetAnalyzer;
		/// the ID of the beat to listen to
		public int BeatID;

		/// <summary>
		/// On Shake, we output our beat value
		/// </summary>
		protected override void Shake()
		{
			base.Shake();
			CurrentLevel = TargetAnalyzer.Beats[BeatID].CurrentValue * GlobalMultiplier;
		}
	}
}