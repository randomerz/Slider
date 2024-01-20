using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will instantiate the associated object (usually a VFX, but not necessarily), optionnally creating an object pool of them for performance
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to instantiate the object specified in its inspector, at the feedback's position (plus an optional offset). You can also optionally (and automatically) create an object pool at initialization to save on performance. In that case you'll need to specify a pool size (usually the maximum amount of these instantiated objects you plan on having in your scene at each given time).")]
	[FeedbackPath("GameObject/Instantiate Object")]
	public class MMFeedbackInstantiateObject : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the different ways to position the instantiated object :
		/// - FeedbackPosition : object will be instantiated at the position of the feedback, plus an optional offset
		/// - Transform : the object will be instantiated at the specified Transform's position, plus an optional offset
		/// - WorldPosition : the object will be instantiated at the specified world position vector, plus an optional offset
		/// - Script : the position passed in parameters when calling the feedback
		public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		[Header("Instantiate Object")]
		/// the object to instantiate
		[Tooltip("the object to instantiate")]
		[FormerlySerializedAs("VfxToInstantiate")]
		public GameObject GameObjectToInstantiate;

		[Header("Position")]
		/// the chosen way to position the object 
		[Tooltip("the chosen way to position the object")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the chosen way to position the object 
		[Tooltip("the chosen way to position the object")]
		public bool AlsoApplyRotation = false;
		/// the chosen way to position the object 
		[Tooltip("the chosen way to position the object")]
		public bool AlsoApplyScale = false;
		/// the transform at which to instantiate the object
		[Tooltip("the transform at which to instantiate the object")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform TargetTransform;
		/// the transform at which to instantiate the object
		[Tooltip("the transform at which to instantiate the object")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetPosition;
		/// the position offset at which to instantiate the object
		[Tooltip("the position offset at which to instantiate the object")]
		[FormerlySerializedAs("VfxPositionOffset")]
		public Vector3 PositionOffset;

		[Header("Object Pool")]
		/// whether or not we should create automatically an object pool for this object
		[Tooltip("whether or not we should create automatically an object pool for this object")]
		[FormerlySerializedAs("VfxCreateObjectPool")]
		public bool CreateObjectPool;
		/// the initial and planned size of this object pool
		[Tooltip("the initial and planned size of this object pool")]
		[MMFCondition("CreateObjectPool", true)]
		[FormerlySerializedAs("VfxObjectPoolSize")]
		public int ObjectPoolSize = 5;
		/// whether or not to create a new pool even if one already exists for that same prefab
		[Tooltip("whether or not to create a new pool even if one already exists for that same prefab")]
		[MMFCondition("CreateObjectPool", true)] 
		public bool MutualizePools = false;

		protected MMMiniObjectPooler _objectPooler; 
		protected GameObject _newGameObject;
		protected bool _poolCreatedOrFound = false;

		/// <summary>
		/// On init we create an object pool if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			if (Active && CreateObjectPool && !_poolCreatedOrFound)
			{
				if (_objectPooler != null)
				{
					_objectPooler.DestroyObjectPool();
					Destroy(_objectPooler.gameObject);
				}

				GameObject objectPoolGo = new GameObject();
				objectPoolGo.name = this.name+"_ObjectPooler";
				_objectPooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
				_objectPooler.GameObjectToPool = GameObjectToInstantiate;
				_objectPooler.PoolSize = ObjectPoolSize;
				_objectPooler.transform.SetParent(this.transform);
				_objectPooler.MutualizeWaitingPools = MutualizePools;
				_objectPooler.FillObjectPool();
				if ((owner != null) && (objectPoolGo.transform.parent == null))
				{
					SceneManager.MoveGameObjectToScene(objectPoolGo, owner.scene);    
				}
				_poolCreatedOrFound = true;
			}
		}

		/// <summary>
		/// On Play we instantiate the specified object, either from the object pool or from scratch
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (GameObjectToInstantiate == null))
			{
				return;
			}
            
			if (_objectPooler != null)
			{
				_newGameObject = _objectPooler.GetPooledGameObject();
				if (_newGameObject != null)
				{
					PositionObject(position);
					_newGameObject.SetActive(true);
				}
			}
			else
			{
				_newGameObject = GameObject.Instantiate(GameObjectToInstantiate) as GameObject;
				if (_newGameObject != null)
				{
					SceneManager.MoveGameObjectToScene(_newGameObject, this.gameObject.scene);
					PositionObject(position);    
				}
			}
		}

		protected virtual void PositionObject(Vector3 position)
		{
			_newGameObject.transform.position = GetPosition(position);
			if (AlsoApplyRotation)
			{
				_newGameObject.transform.rotation = GetRotation();    
			}
			if (AlsoApplyScale)
			{
				_newGameObject.transform.localScale = GetScale();    
			}
		}

		/// <summary>
		/// Gets the desired position of that particle system
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return this.transform.position + PositionOffset;
				case PositionModes.Transform:
					return TargetTransform.position + PositionOffset;
				case PositionModes.WorldPosition:
					return TargetPosition + PositionOffset;
				case PositionModes.Script:
					return position + PositionOffset;
				default:
					return position + PositionOffset;
			}
		}

        
		/// <summary>
		/// Gets the desired rotation of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Quaternion GetRotation()
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return this.transform.rotation;
				case PositionModes.Transform:
					return TargetTransform.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return this.transform.rotation;
				default:
					return this.transform.rotation;
			}
		}

		/// <summary>
		/// Gets the desired scale of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Vector3 GetScale()
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return this.transform.localScale;
				case PositionModes.Transform:
					return TargetTransform.localScale;
				case PositionModes.WorldPosition:
					return this.transform.localScale;
				case PositionModes.Script:
					return this.transform.localScale;
				default:
					return this.transform.localScale;
			}
		}
	}
}