using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	public class MMMiniObjectPooler : MonoBehaviour
	{
		/// the game object we'll instantiate 
		public GameObject GameObjectToPool;
		/// the number of objects we'll add to the pool
		public int PoolSize = 20;
		/// if true, the pool will automatically add objects to the itself if needed
		public bool PoolCanExpand = true;
		/// if this is true, the pool will try not to create a new waiting pool if it finds one with the same name.
		public bool MutualizeWaitingPools = false;
		/// if this is true, all waiting and active objects will be regrouped under an empty game object. Otherwise they'll just be at top level in the hierarchy
		public bool NestWaitingPool = true;
        
		/// this object is just used to group the pooled objects
		protected GameObject _waitingPool = null;
		protected MMMiniObjectPool _objectPool;
		protected const int _initialPoolsListCapacity = 5;
        
		static List<MMMiniObjectPool> _pools = new List<MMMiniObjectPool>(_initialPoolsListCapacity);

		/// <summary>
		/// Adds a pooler to the static list if needed
		/// </summary>
		/// <param name="pool"></param>
		public static void AddPool(MMMiniObjectPool pool)
		{
			if (_pools == null)
			{
				_pools = new List<MMMiniObjectPool>(_initialPoolsListCapacity);    
			}
			if (!_pools.Contains(pool))
			{
				_pools.Add(pool);
			}
		}

		/// <summary>
		/// Removes a pooler from the static list
		/// </summary>
		/// <param name="pool"></param>
		public static void RemovePool(MMMiniObjectPool pool)
		{
			_pools?.Remove(pool);
		}

		/// <summary>
		/// On awake we fill our object pool
		/// </summary>
		protected virtual void Awake()
		{
			FillObjectPool();
		}
        
		/// <summary>
		/// On Destroy we remove ourselves from the list of poolers 
		/// </summary>
		private void OnDestroy()
		{
			if (_objectPool != null)
			{
				RemovePool(_objectPool);    
			}
		}
        
		/// <summary>
		/// Looks for an existing pooler for the same object, returns it if found, returns null otherwise
		/// </summary>
		/// <param name="objectToPool"></param>
		/// <returns></returns>
		public virtual MMMiniObjectPool ExistingPool(string poolName)
		{
			if (_pools == null)
			{
				_pools = new List<MMMiniObjectPool>(_initialPoolsListCapacity);    
			}
            
			if (_pools.Count == 0)
			{
				var pools = FindObjectsOfType<MMMiniObjectPool>();
				if (pools.Length > 0)
				{
					_pools.AddRange(pools);
				}
			}
			foreach (MMMiniObjectPool pool in _pools)
			{
				if ((pool != null) && (pool.name == poolName)/* && (pool.gameObject.scene == this.gameObject.scene)*/)
				{
					return pool;
				}
			}
			return null;
		}

		/// <summary>
		/// Creates the waiting pool or tries to reuse one if there's already one available
		/// </summary>
		protected virtual void CreateWaitingPool()
		{
			if (!MutualizeWaitingPools)
			{
				// we create a container that will hold all the instances we create
				_objectPool = this.gameObject.AddComponent<MMMiniObjectPool>();
				_objectPool.PooledGameObjects = new List<GameObject>();
				return;
			}
			else
			{
				MMMiniObjectPool waitingPool = ExistingPool(DetermineObjectPoolName(GameObjectToPool));
                
				if (waitingPool != null)
				{
					_waitingPool = waitingPool.gameObject;
					_objectPool = waitingPool;
				}
				else
				{
					GameObject newPool = new GameObject();
					newPool.name = DetermineObjectPoolName(GameObjectToPool);
					_objectPool = newPool.AddComponent<MMMiniObjectPool>();
					_objectPool.PooledGameObjects = new List<GameObject>();
					AddPool(_objectPool);
				}
			}
		}

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		public static string DetermineObjectPoolName(GameObject gameObjectToPool)
		{
			return (gameObjectToPool.name + "_pool");
		}

		/// <summary>
		/// Implement this method to fill the pool with objects
		/// </summary>
		public virtual void FillObjectPool()
		{
			if (GameObjectToPool == null)
			{
				return;
			}

			CreateWaitingPool();

			int objectsToSpawn = PoolSize;

			if (_objectPool != null)
			{
				objectsToSpawn -= _objectPool.PooledGameObjects.Count;
			}

			// we add to the pool the specified number of objects
			for (int i = 0; i < objectsToSpawn; i++)
			{
				AddOneObjectToThePool();
			}
		}

		/// <summary>
		/// Implement this method to return a gameobject
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public virtual GameObject GetPooledGameObject()
		{
			// we go through the pool looking for an inactive object
			for (int i = 0; i < _objectPool.PooledGameObjects.Count; i++)
			{
				if (!_objectPool.PooledGameObjects[i].gameObject.activeInHierarchy)
				{
					// if we find one, we return it
					return _objectPool.PooledGameObjects[i];
				}
			}
			// if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		
			if (PoolCanExpand)
			{
				return AddOneObjectToThePool();
			}
			// if the pool is empty and can't grow, we return nothing.
			return null;
		}

		/// <summary>
		/// Adds one object of the specified type (in the inspector) to the pool.
		/// </summary>
		/// <returns>The one object to the pool.</returns>
		protected virtual GameObject AddOneObjectToThePool()
		{
			if (GameObjectToPool == null)
			{
				Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any GameObjectToPool defined.", gameObject);
				return null;
			}
			GameObjectToPool.gameObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(GameObjectToPool);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_objectPool.transform);
			}
			newGameObject.name = GameObjectToPool.name + "-" + _objectPool.PooledGameObjects.Count;

			_objectPool.PooledGameObjects.Add(newGameObject);

			return newGameObject;
		}

		/// <summary>
		/// Destroys the object pool
		/// </summary>
		public virtual void DestroyObjectPool()
		{
			if (_waitingPool != null)
			{
				Destroy(_waitingPool.gameObject);
			}
		}
	}


	public class MMMiniObjectPool : MonoBehaviour
	{
		[MMFReadOnly]
		public List<GameObject> PooledGameObjects;
	}
}