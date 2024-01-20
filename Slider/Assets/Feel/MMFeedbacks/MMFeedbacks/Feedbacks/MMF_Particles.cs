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
	public class MMF_Particles : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundParticleSystem == null); }
		public override string RequiredTargetText { get { return BoundParticleSystem != null ? BoundParticleSystem.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundParticleSystem be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundParticleSystem = FindAutomatedTarget<ParticleSystem>();
        
		public enum Modes { Play, Stop, Pause, Emit }

		[MMFInspectorGroup("Bound Particles", true, 41, true)]
		/// whether to Play, Stop or Pause the target particle system when that feedback is played
		[Tooltip("whether to Play, Stop or Pause the target particle system when that feedback is played")]
		public Modes Mode = Modes.Play;
		/// in Emit mode, the amount of particles per emit
		[Tooltip("in Emit mode, the amount of particles per emit")]
		[MMFEnumCondition("Mode", (int)Modes.Emit)]
		public int EmitCount = 100;
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
		/// if this is true, the particle system will be stopped on initialization
		[Tooltip("if this is true, the particle system will be stopped on initialization")]
		public bool StopSystemOnInit = true;
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("Simulation Speed", true, 43, false)]
		/// whether or not to force a specific simulation speed on the target particle system(s)
		[Tooltip("whether or not to force a specific simulation speed on the target particle system(s)")]
		public bool ForceSimulationSpeed = false;
		/// The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays
		[Tooltip("The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays")]
		[MMFCondition("ForceSimulationSpeed", true)]
		public Vector2 ForcedSimulationSpeed = new Vector2(0.1f,1f);

		protected ParticleSystem.EmitParams _emitParams;

		/// <summary>
		/// On init we stop our particle system
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (StopSystemOnInit)
			{
				StopParticles();
			}
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
				if (Mode != Modes.Emit)
				{
					BoundParticleSystem.transform.position = position;
					foreach (ParticleSystem system in RandomParticleSystems)
					{
						system.transform.position = position;
					}	
				}
				else
				{
					_emitParams.position = position;
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
				HandleParticleSystemAction(RandomParticleSystems[random]);
			}
			else if (BoundParticleSystem != null)
			{
				HandleParticleSystemAction(BoundParticleSystem);
			}
		}

		/// <summary>
		/// Changes the target particle system's sim speed if needed, and calls the specified action on it
		/// </summary>
		/// <param name="targetParticleSystem"></param>
		protected virtual void HandleParticleSystemAction(ParticleSystem targetParticleSystem)
		{
			if (ForceSimulationSpeed)
			{
				ParticleSystem.MainModule main = targetParticleSystem.main;
				main.simulationSpeed = Random.Range(ForcedSimulationSpeed.x, ForcedSimulationSpeed.y);
			}
			
			switch (Mode)
			{
				case Modes.Play:
					targetParticleSystem?.Play();
					break;
				case Modes.Emit:
					_emitParams.applyShapeToPosition = true;
					targetParticleSystem.Emit(_emitParams, EmitCount);
					break;
				case Modes.Stop:
					targetParticleSystem?.Stop();
					break;
				case Modes.Pause:
					targetParticleSystem?.Pause();
					break;
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