using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class lets you randomly spawn objects within its bounds (defined by a 3D collider)
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class MMRandomBoundsInstantiator : MonoBehaviour
	{
		/// the possible start modes
		public enum StartModes { Awake, Start, None }
		/// the possible scale modes you can use to rescale objects
		public enum ScaleModes { Uniform, Vector3 }

		[Header("Random instantiation")]
		/// whether this instantiator should auto trigger on Awake, Start, or never
		public StartModes StartMode = StartModes.Awake;
		/// the name to give to the instantiated object
		public string InstantiatedObjectName = "RandomInstantiated";
		/// if this is true, the instantiated object will be parented to the spawner
		public bool ParentInstantiatedToThisObject = true;
		/// if this is true, every time InstantiateRandomObject is called, any previously instantiated object will be destroyed
		public bool DestroyPreviouslyInstantiatedObjects = true;

		[Header("Spawn")]
		/// the list containing all the objects that can potentially be instantiated
		public List<GameObject> RandomPool;
		/// the min and max bounds to use to determine a random quantity of objects to spawn
		[MMVector("Min", "Max")]
		public Vector2Int Quantity = new Vector2Int(1, 1);

		[Header("Scale")]
		/// the scale mode to use (uniform scales the whole object, Vector3 randomizes x, y and z scale elements
		public ScaleModes ScaleMode = ScaleModes.Uniform;
		/// the min scale to use in uniform mode
		[MMEnumCondition("ScaleMode", (int)ScaleModes.Uniform)]
		public float MinScale = 1f;
		/// the max scale to use in uniform mode
		[MMEnumCondition("ScaleMode", (int)ScaleModes.Uniform)]
		public float MaxScale = 1f;

		/// the min scale to use in vector3 mode
		[MMEnumCondition("ScaleMode", (int)ScaleModes.Vector3)]
		public Vector3 MinVectorScale = Vector3.one;
		/// the max scale to use in vector3 mode
		[MMEnumCondition("ScaleMode", (int)ScaleModes.Vector3)]
		public Vector3 MaxVectorScale = Vector3.one;

		[Header("Test")]
		/// a test button for your inspector
		[MMInspectorButton("Instantiate")]
		public bool InstantiateButton;

		protected Collider _collider;
		protected List<GameObject> _instantiatedGameObjects;
		protected Vector3 _newScale = Vector3.zero;

		/// <summary>
		/// On awake we instantiate if needed
		/// </summary>
		protected virtual void Awake()
		{
			_collider = this.gameObject.GetComponent<Collider>();

			if (StartMode == StartModes.Awake)
			{
				Instantiate();
			}
		}

		/// <summary>
		/// On Start we instantiate if needed
		/// </summary>
		protected virtual void Start()
		{
			if (StartMode == StartModes.Start)
			{
				Instantiate();
			}
		}

		/// <summary>
		/// Instantiates as many objects as needed, clearing previously existing ones if needed
		/// </summary>
		protected virtual void Instantiate()
		{
			if (_instantiatedGameObjects == null)
			{
				_instantiatedGameObjects = new List<GameObject>();
			}

			// we destroy our previous object if needed
			if (DestroyPreviouslyInstantiatedObjects)
			{
				foreach(GameObject go in _instantiatedGameObjects)
				{
					DestroyImmediate(go);
				}
				_instantiatedGameObjects.Clear();
			}

			int random = Random.Range(Quantity.x, Quantity.y);
			for (int i = 0; i < random; i++)
			{
				InstantiateRandomObject();
			}
		}

		/// <summary>
		/// Spawns a random object from the pool of choices
		/// </summary>
		public virtual void InstantiateRandomObject()
		{
			// if the pool is empty we do nothing and exit
			if (RandomPool.Count == 0)
			{
				return;
			}

			// pick a random object and instantiates it
			int randomIndex = Random.Range(0, RandomPool.Count);
			GameObject obj = Instantiate(RandomPool[randomIndex], this.transform.position, this.transform.rotation);
			SceneManager.MoveGameObjectToScene(obj.gameObject, this.gameObject.scene);

			// we pick a random point within the bounds then move it to account for rotation/scale
			obj.transform.position = MMBoundsExtensions.MMRandomPointInBounds(_collider.bounds);
			obj.transform.position = _collider.ClosestPoint(obj.transform.position);

			// we name and parent our object
			obj.name = InstantiatedObjectName;
			if (ParentInstantiatedToThisObject)
			{
				obj.transform.SetParent(this.transform);
			}

			// we rescale the object
			switch (ScaleMode)
			{
				case ScaleModes.Uniform:
					float newScale = Random.Range(MinScale, MaxScale);
					obj.transform.localScale = Vector3.one * newScale;
					break;
				case ScaleModes.Vector3:
					_newScale = MMMaths.RandomVector3(MinVectorScale, MaxVectorScale);
					obj.transform.localScale = _newScale;
					break;
			}

			// we add it to our list
			_instantiatedGameObjects.Add(obj);
		}
	}
}