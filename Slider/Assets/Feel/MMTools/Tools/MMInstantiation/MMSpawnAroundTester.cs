using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A tester class used to show how the MMSpawnAround class can be used
	/// </summary>
	public class MMSpawnAroundTester : MonoBehaviour
	{
		/// a GameObject to instantiate and position around this object 
		public GameObject ObjectToInstantiate;
		/// the spawn properties to consider when spawning the ObjectToInstantiate
		public MMSpawnAroundProperties SpawnProperties;

		[Header("Debug")]
		/// the amount of objects to spawn
		public int DebugQuantity = 10000;
        
		/// a test button
		[MMInspectorButton("DebugSpawn")]
		public bool DebugSpawnButton;

		[Header("Gizmos")]
		/// whether or not to draw gizmos to show the shape of the spawn area
		public bool DrawGizmos = false;
		/// the amount of gizmos to draw
		public int GizmosQuantity = 1000;
		/// the size at which to draw the gizmos
		public float GizmosSize = 1f;
        
		protected GameObject _gameObject;

		/// <summary>
		/// A test method that spawns DebugQuantity objects
		/// </summary>
		public virtual void DebugSpawn()
		{
			for (int i = 0; i < DebugQuantity; i++)
			{
				Spawn();
			}
		}

		/// <summary>
		/// Spawns a single object and positions it correctly
		/// </summary>
		public virtual void Spawn()
		{
			_gameObject = Instantiate(ObjectToInstantiate);
			SceneManager.MoveGameObjectToScene(_gameObject, this.gameObject.scene);
			MMSpawnAround.ApplySpawnAroundProperties(_gameObject, SpawnProperties, this.transform.position);
		}

		/// <summary>
		/// OnDrawGizmos, we draw the shape of the area within which objects will spawn
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (DrawGizmos)
			{
				MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GizmosSize, Color.gray);    
			}
		}
	}
}