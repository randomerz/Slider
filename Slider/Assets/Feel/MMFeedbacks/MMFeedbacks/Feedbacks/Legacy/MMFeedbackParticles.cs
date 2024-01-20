using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will play the associated particles system on play, and stop it on stop
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will simply play the specified ParticleSystem (from your scene) when played.")]
	[FeedbackPath("Particles/Particles Play")]
	public class MMFeedbackParticles : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		#endif
        
		public enum Modes { Play, Stop, Pause }

		[Header("Bound Particles")]
		/// whether to Play, Stop or Pause the target particle system when that feedback is played
		[Tooltip("whether to Play, Stop or Pause the target particle system when that feedback is played")]
		public Modes Mode = Modes.Play;
		/// the particle system to play with this feedback
		[Tooltip("the particle system to play with this feedback")]
		public ParticleSystem BoundParticleSystem;
		/// a list of (optional) particle systems 
		[Tooltip("a list of (optional) particle systems")]
		public List<ParticleSystem> RandomParticleSystems;
		/// if this is true, the particles will be moved to the position passed in parameters
		[Tooltip("if this is true, the particles will be moved to the position passed in parameters")]
		public bool MoveToPosition = false;
		/// if this is true, the particle system's object will be set active on play
		[Tooltip("if this is true, the particle system's object will be set active on play")]
		public bool ActivateOnPlay = false;

		/// <summary>
		/// On init we stop our particle system
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			StopParticles();
		}

		/// <summary>
		/// On play we play our particle system
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			PlayParticles(position);
		}
        
		/// <summary>
		/// On Stop, stops the particle system
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			StopParticles();
		}

		/// <summary>
		/// On Reset, stops the particle system 
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			StopParticles();
		}

		/// <summary>
		/// Plays a particle system
		/// </summary>
		/// <param name="position"></param>
		protected virtual void PlayParticles(Vector3 position)
		{
			if (MoveToPosition)
			{
				BoundParticleSystem.transform.position = position;
				foreach (ParticleSystem system in RandomParticleSystems)
				{
					system.transform.position = position;
				}
			}

			if (ActivateOnPlay)
			{
				BoundParticleSystem.gameObject.SetActive(true);
				foreach (ParticleSystem system in RandomParticleSystems)
				{
					system.gameObject.SetActive(true);
				}
			}

			if (RandomParticleSystems.Count > 0)
			{
				int random = Random.Range(0, RandomParticleSystems.Count);
				switch (Mode)
				{
					case Modes.Play:
						RandomParticleSystems[random].Play();
						break;
					case Modes.Stop:
						RandomParticleSystems[random].Stop();
						break;
					case Modes.Pause:
						RandomParticleSystems[random].Pause();
						break;
				}
				return;
			}
			else if (BoundParticleSystem != null)
			{
				switch (Mode)
				{
					case Modes.Play:
						BoundParticleSystem?.Play();
						break;
					case Modes.Stop:
						BoundParticleSystem?.Stop();
						break;
					case Modes.Pause:
						BoundParticleSystem?.Pause();
						break;
				}
			}
		}

		/// <summary>
		/// Stops all particle systems
		/// </summary>
		protected virtual void StopParticles()
		{
			foreach(ParticleSystem system in RandomParticleSystems)
			{
				system?.Stop();
			}
			if (BoundParticleSystem != null)
			{
				BoundParticleSystem.Stop();
			}            
		}
	}
}