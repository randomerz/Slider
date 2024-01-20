using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will instantiate a particle system and play/stop it when playing/stopping the feedback
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will instantiate the specified ParticleSystem at the specified position on Start or on Play, optionally nesting them.")]
	[FeedbackPath("Particles/Particles Instantiation")]
	public class MMF_ParticlesInstantiation : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		public override bool EvaluateRequiresSetup() { return (ParticlesPrefab == null); }
		public override string RequiredTargetText { get { return ParticlesPrefab != null ? ParticlesPrefab.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a ParticlesPrefab be set to be able to work properly. You can set one below."; } }
		#endif
		/// the different ways to position the instantiated object :
		/// - FeedbackPosition : object will be instantiated at the position of the feedback, plus an optional offset
		/// - Transform : the object will be instantiated at the specified Transform's position, plus an optional offset
		/// - WorldPosition : the object will be instantiated at the specified world position vector, plus an optional offset
		/// - Script : the position passed in parameters when calling the feedback
		public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }
		/// the possible delivery modes
		/// - cached : will cache a copy of the particle system and reuse it
		/// - on demand : will instantiate a new particle system for every play
		public enum Modes { Cached, OnDemand, Pool }

		[MMFInspectorGroup("Particles Instantiation", true, 37, true)]
		/// whether the particle system should be cached or created on demand the first time
		[Tooltip("whether the particle system should be cached or created on demand the first time")]
		public Modes Mode = Modes.Cached;
		
		/// the initial and planned size of this object pool
		[Tooltip("the initial and planned size of this object pool")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public int ObjectPoolSize = 5;
		/// whether or not to create a new pool even if one already exists for that same prefab
		[Tooltip("whether or not to create a new pool even if one already exists for that same prefab")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public bool MutualizePools = false;
		/// if specified, the instantiated object (or the pool of objects) will be parented to this transform 
		[Tooltip("if specified, the instantiated object (or the pool of objects) will be parented to this transform ")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public Transform ParentTransform;
		
		/// if this is false, a brand new particle system will be created every time
		[Tooltip("if this is false, a brand new particle system will be created every time")]
		[MMFEnumCondition("Mode", (int)Modes.OnDemand)]
		public bool CachedRecycle = true;
		/// the particle system to spawn
		[Tooltip("the particle system to spawn")]
		public ParticleSystem ParticlesPrefab;
		/// the possible random particle systems
		[Tooltip("the possible random particle systems")]
		public List<ParticleSystem> RandomParticlePrefabs;
		/// if this is true, the particle system game object will be activated on Play, useful if you've somehow disabled it in a past Play
		[Tooltip("if this is true, the particle system game object will be activated on Play, useful if you've somehow disabled it in a past Play")]
		public bool ForceSetActiveOnPlay = false;
		/// if this is true, the particle system will be stopped every time the feedback is reset - usually before play
		[Tooltip("if this is true, the particle system will be stopped every time the feedback is reset - usually before play")]
		public bool StopOnReset = false;
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("Position", true, 29)]
		/// the selected position mode
		[Tooltip("the selected position mode")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the position at which to spawn this particle system
		[Tooltip("the position at which to spawn this particle system")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform InstantiateParticlesPosition;
		/// the world position to move to when in WorldPosition mode 
		[Tooltip("the world position to move to when in WorldPosition mode")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetWorldPosition;
		/// an offset to apply to the instantiation position
		[Tooltip("an offset to apply to the instantiation position")]
		public Vector3 Offset;
		/// whether or not the particle system should be nested in hierarchy or floating on its own
		[Tooltip("whether or not the particle system should be nested in hierarchy or floating on its own")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform, (int)PositionModes.FeedbackPosition)]
		public bool NestParticles = true;
		/// whether or not to also apply rotation
		[Tooltip("whether or not to also apply rotation")]
		public bool ApplyRotation = false;
		/// whether or not to also apply scale
		[Tooltip("whether or not to also apply scale")]
		public bool ApplyScale = false;

		[MMFInspectorGroup("Simulation Speed", true, 43, false)]
		/// whether or not to force a specific simulation speed on the target particle system(s)
		[Tooltip("whether or not to force a specific simulation speed on the target particle system(s)")]
		public bool ForceSimulationSpeed = false;
		/// The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays
		[Tooltip("The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays")]
		[MMFCondition("ForceSimulationSpeed", true)]
		public Vector2 ForcedSimulationSpeed = new Vector2(0.1f,1f);

		protected ParticleSystem _instantiatedParticleSystem;
		protected List<ParticleSystem> _instantiatedRandomParticleSystems;

		protected MMMiniObjectPooler _objectPooler; 
		protected GameObject _newGameObject;
		protected bool _poolCreatedOrFound = false;
		
		/// <summary>
		/// On init, instantiates the particle system, positions it and nests it if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			if (!Active)
			{
				return;
			}
			
			CacheParticleSystem();

			CreatePools(owner);
		}
		
		protected virtual bool ShouldCache => (Mode == Modes.OnDemand && CachedRecycle) || (Mode == Modes.Cached);

		protected virtual void CreatePools(MMF_Player owner)
		{
			if (Mode != Modes.Pool)
			{
				return;
			}

			if (!_poolCreatedOrFound)
			{
				if (_objectPooler != null)
				{
					_objectPooler.DestroyObjectPool();
					owner.ProxyDestroy(_objectPooler.gameObject);
				}

				GameObject objectPoolGo = new GameObject();
				objectPoolGo.name = Owner.name+"_ObjectPooler";
				_objectPooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
				_objectPooler.GameObjectToPool = ParticlesPrefab.gameObject;
				_objectPooler.PoolSize = ObjectPoolSize;
				_objectPooler.NestWaitingPool = NestParticles;
				if (ParentTransform != null)
				{
					_objectPooler.transform.SetParent(ParentTransform);
				}
				else
				{
					_objectPooler.transform.SetParent(Owner.transform);
				}
				_objectPooler.MutualizeWaitingPools = MutualizePools;
				_objectPooler.FillObjectPool();
				if ((Owner != null) && (objectPoolGo.transform.parent == null))
				{
					SceneManager.MoveGameObjectToScene(objectPoolGo, Owner.gameObject.scene);    
				}
				_poolCreatedOrFound = true;
			}
			
		}
		
		protected virtual void CacheParticleSystem()
		{
			if (!ShouldCache)
			{
				return;
			}

			InstantiateParticleSystem();
		}

		/// <summary>
		/// Instantiates the particle system
		/// </summary>
		protected virtual void InstantiateParticleSystem()
		{
			Transform newParent = null;
            
			if (NestParticles)
			{
				if (PositionMode == PositionModes.FeedbackPosition)
				{
					newParent = Owner.transform;
				}
				if (PositionMode == PositionModes.Transform)
				{
					newParent = InstantiateParticlesPosition;
				}
			}
			
			if (RandomParticlePrefabs.Count > 0)
			{
				if (ShouldCache)
				{
					_instantiatedRandomParticleSystems = new List<ParticleSystem>();
					foreach(ParticleSystem system in RandomParticlePrefabs)
					{
						ParticleSystem newSystem = GameObject.Instantiate(system, newParent) as ParticleSystem;
						if (newParent == null)
						{
							SceneManager.MoveGameObjectToScene(newSystem.gameObject, Owner.gameObject.scene);    
						}
						newSystem.Stop();
						_instantiatedRandomParticleSystems.Add(newSystem);
					}
				}
				else
				{
					int random = Random.Range(0, RandomParticlePrefabs.Count);
					_instantiatedParticleSystem = GameObject.Instantiate(RandomParticlePrefabs[random], newParent) as ParticleSystem;
					if (newParent == null)
					{
						SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, Owner.gameObject.scene);    
					}
				}
			}
			else
			{
				if (ParticlesPrefab == null)
				{
					return;
				}
				_instantiatedParticleSystem = GameObject.Instantiate(ParticlesPrefab, newParent) as ParticleSystem;
				_instantiatedParticleSystem.Stop();
				if (newParent == null)
				{
					SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, Owner.gameObject.scene);    
				}
			}
			
			if (_instantiatedParticleSystem != null)
			{
				PositionParticleSystem(_instantiatedParticleSystem);
			}

			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					PositionParticleSystem(system);
				}
			}
		}

		protected virtual void PositionParticleSystem(ParticleSystem system)
		{
			if (InstantiateParticlesPosition == null)
			{
				if (Owner != null)
				{
					InstantiateParticlesPosition = Owner.transform;
				}
			}

			if (system != null)
			{
				system.Stop();
				
				system.transform.position = GetPosition(Owner.transform.position);
				if (ApplyRotation)
				{
					system.transform.rotation = GetRotation(Owner.transform);    
				}

				if (ApplyScale)
				{
					system.transform.localScale = GetScale(Owner.transform);    
				}
            
				system.Clear();
			}
		}

		/// <summary>
		/// Gets the desired rotation of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Quaternion GetRotation(Transform target)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.rotation;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return Owner.transform.rotation;
				default:
					return Owner.transform.rotation;
			}
		}

		/// <summary>
		/// Gets the desired scale of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Vector3 GetScale(Transform target)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.localScale;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.localScale;
				case PositionModes.WorldPosition:
					return Owner.transform.localScale;
				case PositionModes.Script:
					return Owner.transform.localScale;
				default:
					return Owner.transform.localScale;
			}
		}

		/// <summary>
		/// Gets the position 
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + Offset;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.position + Offset;
				case PositionModes.WorldPosition:
					return TargetWorldPosition + Offset;
				case PositionModes.Script:
					return position + Offset;
				default:
					return position + Offset;
			}
		}

		/// <summary>
		/// On Play, plays the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.Pool)
			{
				if (_objectPooler != null)
				{
					_newGameObject = _objectPooler.GetPooledGameObject();
					_instantiatedParticleSystem = _newGameObject.MMFGetComponentNoAlloc<ParticleSystem>();
					if (_instantiatedParticleSystem != null)
					{
						PositionParticleSystem(_instantiatedParticleSystem);
						_newGameObject.SetActive(true);
					}
				}
			}
			else
			{
				if (!ShouldCache)
				{
					InstantiateParticleSystem();
				}
				else
				{
					GrabCachedParticleSystem();
				}
			}
			
			if (_instantiatedParticleSystem != null)
			{
				if (ForceSetActiveOnPlay)
				{
					_instantiatedParticleSystem.gameObject.SetActive(true);
				}
				_instantiatedParticleSystem.Stop();
				_instantiatedParticleSystem.transform.position = GetPosition(position);
				PlayTargetParticleSystem(_instantiatedParticleSystem);
			}

			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
                    
					if (ForceSetActiveOnPlay)
					{
						system.gameObject.SetActive(true);
					}
					system.Stop();
					system.transform.position = GetPosition(position);
				}
				int random = Random.Range(0, _instantiatedRandomParticleSystems.Count);
				PlayTargetParticleSystem(_instantiatedRandomParticleSystems[random]);
			}
		}

		/// <summary>
		/// Forces the sim speed if needed, then plays the target particle system
		/// </summary>
		/// <param name="targetParticleSystem"></param>
		protected virtual void PlayTargetParticleSystem(ParticleSystem targetParticleSystem)
		{
			if (ForceSimulationSpeed)
			{
				ParticleSystem.MainModule main = targetParticleSystem.main;
				main.simulationSpeed = Random.Range(ForcedSimulationSpeed.x, ForcedSimulationSpeed.y);
			}
			targetParticleSystem.Play();
		}

		/// <summary>
		/// Grabs and stores a random particle prefab
		/// </summary>
		protected virtual void GrabCachedParticleSystem()
		{
			if (RandomParticlePrefabs.Count > 0)
			{
				int random = Random.Range(0, RandomParticlePrefabs.Count);
				_instantiatedParticleSystem = _instantiatedRandomParticleSystems[random];
			}
		}

		/// <summary>
		/// On Stop, stops the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (_instantiatedParticleSystem != null)
			{
				_instantiatedParticleSystem?.Stop();
			}    
			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach(ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					system.Stop();
				}
			}
		}

		/// <summary>
		/// On Reset, stops the feedback
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
            
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (InCooldown)
			{
				return;
			}

			if (StopOnReset && (_instantiatedParticleSystem != null))
			{
				_instantiatedParticleSystem.Stop();
			}
			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					system.Stop();
				}
			}
		}
	}
}