using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{	
	[Serializable]
	/// <summary>
	/// Multiple object pooler object.
	/// </summary>
	public class MMMultipleObjectPoolerObject
	{
		public GameObject GameObjectToPool;
		public int PoolSize;
		public bool PoolCanExpand = true;
		public bool Enabled = true;
	}

	/// <summary>
	/// The various methods you can pull objects from the pool with
	/// </summary>
	public enum MMPoolingMethods { OriginalOrder, OriginalOrderSequential, RandomBetweenObjects, RandomPoolSizeBased }

	/// <summary>
	/// This class allows you to have a pool of various objects to pool from.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Object Pool/MMMultipleObjectPooler")]
	public class MMMultipleObjectPooler : MMObjectPooler
	{
		/// the list of objects to pool
		public List<MMMultipleObjectPoolerObject> Pool;
		[MMInformation("A MultipleObjectPooler is a reserve of objects, to be used by a Spawner. When asked, it will return an object from the pool (ideally an inactive one) chosen based on the pooling method you've chosen.\n- OriginalOrder will spawn objects in the order you've set them in the inspector (from top to bottom)\n- OriginalOrderSequential will do the same, but will empty each pool before moving to the next object\n- RandomBetweenObjects will pick one object from the pool, at random, but ignoring its pool size, each object has equal chances to get picked\n- PoolSizeBased randomly choses one object from the pool, based on its pool size probability (the larger the pool size, the higher the chances it'll get picked)'...",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the chosen pooling method
		public MMPoolingMethods PoolingMethod = MMPoolingMethods.RandomPoolSizeBased;
		[MMInformation("If you set CanPoolSameObjectTwice to false, the Pooler will try to prevent the same object from being pooled twice to avoid repetition. This will only affect random pooling methods, not ordered pooling.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// whether or not the same object can be pooled twice in a row. If you set CanPoolSameObjectTwice to false, the Pooler will try to prevent the same object from being pooled twice to avoid repetition. This will only affect random pooling methods, not ordered pooling.
		public bool CanPoolSameObjectTwice=true;
		/// a unique name that should match on all MMMultipleObjectPoolers you want to use together
		[MMCondition("MutualizeWaitingPools", true)]
		public string MutualizedPoolName = "";
		
		public List<MMMultipleObjectPooler> Owner { get; set; }
		private void OnDestroy() { Owner?.Remove(this); }
		
		/// the actual object pool
		protected GameObject _lastPooledObject;
		protected int _currentIndex = 0;
		protected int _currentIndexCounter = 0;
		
		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected override string DetermineObjectPoolName()
		{
			if ((MutualizedPoolName == null) || (MutualizedPoolName == ""))
			{
				return ("[MultipleObjectPooler] " + this.name);	
			}
			else
			{
				return ("[MultipleObjectPooler] " + MutualizedPoolName);	
			}
		}

		/// <summary>
		/// Fills the object pool with the amount of objects you specified in the inspector.
		/// </summary>
		public override void FillObjectPool()
		{
			if ((Pool == null) || (Pool.Count == 0))
			{
				return;
			}

			// we create a waiting pool, if one already exists, no need to fill anything
			if (!CreateWaitingPool())
			{
				return;
			}
            
			// if there's only one item in the Pool, we force CanPoolSameObjectTwice to true
			if (Pool.Count <= 1)
			{
				CanPoolSameObjectTwice=true;
			}

			bool stillObjectsToPool;
			int[] poolSizes;

			// if we're gonna pool in the original inspector order
			switch (PoolingMethod)
			{
				case MMPoolingMethods.OriginalOrder:
					stillObjectsToPool = true;
					// we store our poolsizes in a temp array so it doesn't impact the inspector
					poolSizes = new int[Pool.Count];
					for (int i = 0; i < Pool.Count; i++)
					{
						poolSizes[i] = Pool[i].PoolSize;
					}

					// we go through our objects in the order they were in the inspector, and fill the pool while we find objects to add
					while (stillObjectsToPool)
					{
						stillObjectsToPool = false;
						for (int i = 0; i < Pool.Count; i++)
						{
							if (poolSizes[i] > 0)
							{
								AddOneObjectToThePool(Pool[i].GameObjectToPool);
								poolSizes[i]--;
								stillObjectsToPool = true;
							}			            
						}
					}
					break;
				case MMPoolingMethods.OriginalOrderSequential:
					// we store our poolsizes in a temp array so it doesn't impact the inspector
					foreach (MMMultipleObjectPoolerObject pooledGameObject in Pool)
					{
						for (int i = 0; i < pooledGameObject.PoolSize ; i++ )
						{
							AddOneObjectToThePool(pooledGameObject.GameObjectToPool);								
						}
					}				
					break;
				default:
					int k = 0;
					// for each type of object specified in the inspector
					foreach (MMMultipleObjectPoolerObject pooledGameObject in Pool)
					{
						// if there's no specified number of objects to pool for that type of object, we do nothing and exit
						if (k > Pool.Count) { return; }

						// we add, one by one, the number of objects of that type, as specified in the inspector
						for (int j = 0; j < Pool[k].PoolSize; j++)
						{
							AddOneObjectToThePool(pooledGameObject.GameObjectToPool);
						}
						k++;
					}
					break;
			}
		}

		/// <summary>
		/// Adds one object of the specified type to the object pool.
		/// </summary>
		/// <returns>The object that just got added.</returns>
		/// <param name="typeOfObject">The type of object to add to the pool.</param>
		protected virtual GameObject AddOneObjectToThePool(GameObject typeOfObject)
		{
			if (typeOfObject == null)
			{
				return null;
			}

			bool initialStatus = typeOfObject.activeSelf;
			typeOfObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(typeOfObject);
			typeOfObject.SetActive(initialStatus);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_waitingPool.transform);	
			}
			newGameObject.name = typeOfObject.name;
			_objectPool.PooledGameObjects.Add(newGameObject);
			return newGameObject;
		}

		/// <summary>
		/// Gets a random object from the pool.
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override GameObject GetPooledGameObject()
		{
			GameObject pooledGameObject;
			switch (PoolingMethod)
			{
				case MMPoolingMethods.OriginalOrder:
					pooledGameObject = GetPooledGameObjectOriginalOrder();
					break;
				case MMPoolingMethods.RandomPoolSizeBased:
					pooledGameObject =  GetPooledGameObjectPoolSizeBased();
					break;
				case MMPoolingMethods.RandomBetweenObjects:
					pooledGameObject =  GetPooledGameObjectRandomBetweenObjects();
					break;
				case MMPoolingMethods.OriginalOrderSequential:
					pooledGameObject =  GetPooledGameObjectOriginalOrderSequential();
					break;
				default:
					pooledGameObject = null;
					break;
			}
			if (pooledGameObject!=null)
			{
				_lastPooledObject = pooledGameObject;
			}
			else
			{	
				_lastPooledObject = null;
			}
			return pooledGameObject;
		}

		/// <summary>
		/// Tries to find a gameobject in the pool according to the order the list has been setup in (one of each, no matter how big their respective pool sizes)
		/// </summary>
		/// <returns>The pooled game object original order.</returns>
		protected virtual GameObject GetPooledGameObjectOriginalOrder()
		{
			int newIndex;
			// if we've reached the end of our list, we start again from the beginning
			if (_currentIndex >= Pool.Count)
			{
				ResetCurrentIndex ();
			}

			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(Pool[_currentIndex].GameObjectToPool);

			if (_currentIndex >= _objectPool.PooledGameObjects.Count) { return null; }
			if (!searchedObject.Enabled) { _currentIndex++; return null; }

			// if the object is already active, we need to find another one
			if (_objectPool.PooledGameObjects[_currentIndex].gameObject.activeInHierarchy)
			{
				GameObject findObject = FindInactiveObject(_objectPool.PooledGameObjects[_currentIndex].gameObject.name,_objectPool.PooledGameObjects);
				if (findObject != null)
				{
					_currentIndex++;
					return findObject;
				}

				// if its pool can expand, we create a new one
				if (searchedObject.PoolCanExpand)
				{
					_currentIndex++;
					return AddOneObjectToThePool(searchedObject.GameObjectToPool);	
				}
				else
				{
					// if it can't expand we return nothing
					return null;					
				}
			}
			else
			{
				// if the object is inactive, we return it
				newIndex = _currentIndex;
				_currentIndex++;
				return _objectPool.PooledGameObjects[newIndex]; 
			}
		}

		protected int _currentCount = 0;

		/// <summary>
		/// Tries to find a gameobject in the pool according to the order the list has been setup in (one of each, no matter how big their respective pool sizes)
		/// </summary>
		/// <returns>The pooled game object original order.</returns>
		protected virtual GameObject GetPooledGameObjectOriginalOrderSequential()
		{
			// if we've reached the end of our list, we start again from the beginning
			if (_currentIndex >= Pool.Count)
			{
				_currentCount = 0;
				ResetCurrentIndex ();
			}

			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(Pool[_currentIndex].GameObjectToPool);

			if (_currentIndex >= _objectPool.PooledGameObjects.Count) { return null; }
			if (!searchedObject.Enabled) { _currentIndex++; _currentCount = 0; return null; }


			// if the object is already active, we need to find another one
			if (_objectPool.PooledGameObjects[_currentIndex].gameObject.activeInHierarchy)
			{
				GameObject findObject = FindInactiveObject(Pool[_currentIndex].GameObjectToPool.name, _objectPool.PooledGameObjects);
				if (findObject != null)
				{
					_currentCount++;
					OrderSequentialResetCounter(searchedObject);
					return findObject;
				}

				// if its pool can expand, we create a new one
				if (searchedObject.PoolCanExpand)
				{
					_currentCount++;
					OrderSequentialResetCounter(searchedObject);
					return AddOneObjectToThePool(searchedObject.GameObjectToPool);	
				}
				else
				{
					// if it can't expand we return nothing
					_currentIndex++;
					_currentCount = 0;
					return null;					
				}
			}
			else
			{
				// if the object is inactive, we return it
				_currentCount++;
				OrderSequentialResetCounter(searchedObject);
				return _objectPool.PooledGameObjects[_currentIndex]; 
			}
		}

		protected virtual void OrderSequentialResetCounter(MMMultipleObjectPoolerObject searchedObject)
		{
			if (_currentCount >= searchedObject.PoolSize)
			{
				_currentIndex++;
				_currentCount = 0;
			}
		}

		/// <summary>
		/// Randomly choses one object from the pool, based on its pool size probability (the larger the pool size, the higher the chances it'll get picked)
		/// </summary>
		/// <returns>The pooled game object pool size based.</returns>
		protected virtual GameObject GetPooledGameObjectPoolSizeBased()
		{
			// we get a random index 
			int randomIndex = UnityEngine.Random.Range(0, _objectPool.PooledGameObjects.Count);

			int overflowCounter=0;

			// we check to see if that object is enabled, if it's not we loop
			while (!PoolObjectEnabled(_objectPool.PooledGameObjects[randomIndex]) && overflowCounter < _objectPool.PooledGameObjects.Count)
			{
				randomIndex = UnityEngine.Random.Range(0, _objectPool.PooledGameObjects.Count);
				overflowCounter++;
			}
			if (!PoolObjectEnabled(_objectPool.PooledGameObjects[randomIndex]))
			{ 
				return null; 
			}

			// if we can't pool the same object twice, we'll loop for a while to try and get another one
			overflowCounter = 0;
			while (!CanPoolSameObjectTwice 
			       && _objectPool.PooledGameObjects[randomIndex] == _lastPooledObject 
			       && overflowCounter < _objectPool.PooledGameObjects.Count)
			{
				randomIndex = UnityEngine.Random.Range(0, _objectPool.PooledGameObjects.Count);
				overflowCounter++;
			}

			//  if the item we've picked is active
			if (_objectPool.PooledGameObjects[randomIndex].gameObject.activeInHierarchy)
			{	
				// we try to find another inactive object of the same type
				GameObject pulledObject = FindInactiveObject(_objectPool.PooledGameObjects[randomIndex].gameObject.name,_objectPool.PooledGameObjects);
				if (pulledObject!=null)
				{
					return pulledObject;
				}
				else
				{
					// if we couldn't find an inactive object of this type, we see if it can expand
					MMMultipleObjectPoolerObject searchedObject = GetPoolObject(_objectPool.PooledGameObjects[randomIndex].gameObject);
					if (searchedObject==null)
					{
						return null; 
					}
					// if the pool for this object is allowed to grow (this is set in the inspector if you're wondering)
					if (searchedObject.PoolCanExpand)
					{						
						return AddOneObjectToThePool(searchedObject.GameObjectToPool);						 	
					}
					else
					{
						// if it's not allowed to grow, we return nothing.
						return null;
					}
				}
			}
			else
			{			
				// if the pool wasn't empty, we return the random object we've found.
				return _objectPool.PooledGameObjects[randomIndex];   
			}
		}

		/// <summary>
		/// Gets one object from the pool, at random, but ignoring its pool size, each object has equal chances to get picked
		/// </summary>
		/// <returns>The pooled game object random between objects.</returns>
		protected virtual GameObject GetPooledGameObjectRandomBetweenObjects()
		{
			// we pick one of the objects in the original pool at random
			int randomIndex = UnityEngine.Random.Range(0, Pool.Count);
			
			int overflowCounter=0;

			// if we can't pool the same object twice, we'll loop for a while to try and get another one
			while (!CanPoolSameObjectTwice && Pool[randomIndex].GameObjectToPool == _lastPooledObject && overflowCounter < _objectPool.PooledGameObjects.Count )
			{
				randomIndex = UnityEngine.Random.Range(0, Pool.Count);
				overflowCounter++;
			}
			int originalRandomIndex = randomIndex+1;

			bool objectFound = false;
			
			// while we haven't found an object to return, and while we haven't gone through all the different object types, we keep going
			overflowCounter=0;
			while (!objectFound 
			       && randomIndex != originalRandomIndex 
			       && overflowCounter < _objectPool.PooledGameObjects.Count)
			{
				// if our index is at the end, we reset it
				if (randomIndex >= Pool.Count)
				{
					randomIndex=0;
				}

				if (!Pool[randomIndex].Enabled)
				{
					randomIndex++;
					overflowCounter++;
					continue;
				}

				// we try to find an inactive object of that type in the pool
				GameObject newGameObject = FindInactiveObject(Pool[randomIndex].GameObjectToPool.name, _objectPool.PooledGameObjects);
				if (newGameObject!=null)
				{
					objectFound=true;
					return newGameObject;
				}
				else
				{
					// if there's none and if we can expand, we expand
					if (Pool[randomIndex].PoolCanExpand)
					{
						return AddOneObjectToThePool(Pool[randomIndex].GameObjectToPool);	
					}
				}
				randomIndex++;
				overflowCounter++;
			}
			return null;
		}

		protected string _tempSearchedName;
		
		/// <summary>
		/// Gets an object of the type at the specified index in the Pool.
		/// Note that the whole point of this multiple object pooler is to abstract the various pools and handle
		/// the picking based on the selected mode. If you plan on just picking from different pools yourself,
		/// consider simply having multiple single object poolers.
		/// </summary>
		/// <param name="index"></param>
		public virtual GameObject GetPooledGamObjectAtIndex(int index)
		{
			if ((index < 0) || (index >= Pool.Count))
			{
				return null;
			}

			_tempSearchedName = Pool[index].GameObjectToPool.name;
			return GetPooledGameObjectOfType(_tempSearchedName);
		}

		/// <summary>
		/// Gets an object of the specified name from the pool
		/// Note that the whole point of this multiple object pooler is to abstract the various pools and handle
		/// the picking based on the selected mode. If you plan on just picking from different pools yourself,
		/// consider simply having multiple single object poolers.
		/// </summary>
		/// <returns>The pooled game object of type.</returns>
		/// <param name="type">Type.</param>
		public virtual GameObject GetPooledGameObjectOfType(string searchedName)
		{
			GameObject newObject = FindInactiveObject(searchedName,_objectPool.PooledGameObjects);

			if (newObject!=null)
			{
				return newObject;
			}
			else
			{
				// if we've not returned the object, that means the pool is empty (at least it means it doesn't contain any object of that specific type)
				// so if the pool is allowed to expand
				GameObject searchedObject = FindObject(searchedName,_objectPool.PooledGameObjects);
				if (searchedObject == null) 
				{
					return null;
				}

				if (GetPoolObject(FindObject(searchedName,_objectPool.PooledGameObjects)).PoolCanExpand)
				{
					return AddOneObjectToThePool(searchedObject);
				}
			}

			// if the pool was empty for that object and not allowed to expand, we return nothing.
			return null;
		}

		/// <summary>
		/// Finds an inactive object in the pool based on its name.
		/// Returns null if no inactive object by that name were found in the pool
		/// </summary>
		/// <returns>The inactive object.</returns>
		/// <param name="searchedName">Searched name.</param>
		protected virtual GameObject FindInactiveObject(string searchedName, List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				// if we find an object inside the pool that matches the asked type
				if (list[i].name.Equals(searchedName))
				{
					// and if that object is inactive right now
					if (!list[i].gameObject.activeInHierarchy)
					{
						// we return it
						return list[i];
					}
				}            
			}
			return null;
		}

		protected virtual GameObject FindAnyInactiveObject(List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				// and if that object is inactive right now
				if (!list[i].gameObject.activeInHierarchy)
				{
					// we return it
					return list[i];
				}                        
			}
			return null;
		}

		/// <summary>
		/// Finds an object in the pool based on its name, active or inactive
		/// Returns null if there's no object by that name in the pool
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="searchedName">Searched name.</param>
		protected virtual GameObject FindObject(string searchedName,List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				// if we find an object inside the pool that matches the asked type
				if (list[i].name.Equals(searchedName))
				{
					// and if that object is inactive right now
					return list[i];
				}            
			}
			return null;
		}

		/// <summary>
		/// Returns (if it exists) the MultipleObjectPoolerObject from the original Pool based on a GameObject.
		/// Note that this is name based.
		/// </summary>
		/// <returns>The pool object.</returns>
		/// <param name="testedObject">Tested object.</param>
		protected virtual MMMultipleObjectPoolerObject GetPoolObject(GameObject testedObject)
		{
			if (testedObject==null)
			{
				return null;
			}
			int i=0;
			foreach(MMMultipleObjectPoolerObject poolerObject in Pool)
			{
				if (testedObject.name.Equals(poolerObject.GameObjectToPool.name))
				{
					return (poolerObject);
				}
				i++;
			}
			return null;
		}

		protected virtual bool PoolObjectEnabled(GameObject testedObject)
		{
			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(testedObject);
			if (searchedObject != null)
			{
				return searchedObject.Enabled;
			}
			else
			{
				return false;
			}
		}

		public virtual void EnableObjects(string name,bool newStatus)
		{
			foreach(MMMultipleObjectPoolerObject poolerObject in Pool)
			{
				if (name.Equals(poolerObject.GameObjectToPool.name))
				{
					poolerObject.Enabled = newStatus;
				}
			}
		}

		public virtual void ResetCurrentIndex()
		{
			_currentIndex = 0;
			_currentIndexCounter = 0;
		}
	}
}