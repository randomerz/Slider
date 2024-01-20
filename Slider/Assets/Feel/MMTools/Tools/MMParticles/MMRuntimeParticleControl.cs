using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this class to a particle system at runtime, and it'll expose controls to play/pause/stop it from the inspector
	/// Because Unity's built-in controls somehow lack pause when in play mode
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class MMRuntimeParticleControl : MonoBehaviour
	{
		/// <summary>
		/// The possible modes for the tracker :
		/// Basic will work with the main module's duration
		/// ForcedBounds lets you specify within which bounds the slider should move
		/// </summary>
		public enum TrackerModes { Basic, ForcedBounds }
        
		[Header("Base Controls")]
		/// a test button to play the associated particle system 
		[MMInspectorButton("Play")] public bool PlayButton;
		/// a test button to pause the associated particle system
		[MMInspectorButton("Pause")] public bool PauseButton;
		/// a test button to stop the associated particle system
		[MMInspectorButton("Stop")] public bool StopButton;
        
		[Header("Simulate")]
		/// the timestamp at which to go when pressing the Simulate button
		public float TargetTimestamp = 1f;
		/// a test button to move the associated particle system to the specified timestamp
		[MMInspectorButton("Simulate")] public bool FastForwardToTimeButton;

		[Header("Tracker")]
		/// the selected tracker mode
		public TrackerModes TrackerMode = TrackerModes.Basic;
		/// when in ForcedBounds mode, the value to which the slider's lowest bound should be remapped
		[MMEnumCondition("TrackerMode", (int)TrackerModes.ForcedBounds)]
		public float MinBound;
		/// when in ForcedBounds mode, the value to which the slider's highest bound should be remapped
		[MMEnumCondition("TrackerMode", (int)TrackerModes.ForcedBounds)]
		public float MaxBound;
		/// a slider used to move the particle system through time at runtime
		[Range(0f, 1f)]
		public float Tracker;
		[MMReadOnly] 
		public float Timestamp;

		protected ParticleSystem _particleSystem;
		protected ParticleSystem.MainModule _mainModule;
        
		/// <summary>
		/// On Awake we grab our components
		/// </summary>
		protected virtual void Awake()
		{
			_particleSystem = this.GetComponent<ParticleSystem>();
			_mainModule = _particleSystem.main;
		}

		/// <summary>
		/// Plays the particle system
		/// </summary>
		protected virtual void Play()
		{
			_particleSystem.Play();
		}

		/// <summary>
		/// Pauses the particle system
		/// </summary>
		protected virtual void Pause()
		{
			_particleSystem.Pause();
		}

		/// <summary>
		/// Stops the particle system
		/// </summary>
		protected virtual void Stop()
		{
			_particleSystem.Stop();
		}

		/// <summary>
		/// Moves the particle system to the specified timestamp
		/// </summary>
		protected virtual void Simulate()
		{
			_particleSystem.Simulate(TargetTimestamp, true, true);
		}

		/// <summary>
		/// On validate, moves the particle system to the chosen timestamp along the track
		/// </summary>
		protected void OnValidate()
		{
			float minBound = (TrackerMode == TrackerModes.Basic) ? 0f : MinBound;
			float maxBound = (TrackerMode == TrackerModes.Basic) ? _mainModule.duration : MaxBound;
			Timestamp = MMMaths.Remap(Tracker, 0f, 1f, minBound, maxBound);
			_particleSystem.Simulate(Timestamp, true, true);
		}
	}    
}