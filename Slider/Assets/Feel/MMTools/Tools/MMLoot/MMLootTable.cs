using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MoreMountains.Tools
{
	/// <summary>
	/// A loot table helper that can be used to randomly pick objects out of a weighted list
	/// This design pattern was described in more details by Daniel Cook in 2014 in his blog :
	/// https://lostgarden.home.blog/2014/12/08/loot-drop-tables/
	///
	/// This generic LootTable defines a list of objects to loot, each of them weighted.
	/// The weights don't have to add to a certain number, they're relative to each other.
	/// The ComputeWeights method determines, based on these weights, the chance percentage of each object to be picked
	/// The GetLoot method returns one object, picked randomly from the table
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="V"></typeparam>
	public class MMLootTable<T,V> where T:MMLoot<V>
	{
		/// the list of objects that have a chance of being returned by the table
		[SerializeField]
		public List<T> ObjectsToLoot;

		/// the total amount of weights, for debug purposes only
		[Header("Debug")]
		[MMReadOnly]
		public float WeightsTotal;
        
		protected float _maximumWeightSoFar = 0f;
		protected bool _weightsComputed = false;
        
		/// <summary>
		/// Determines, for each object in the table, its chance percentage, based on the specified weights
		/// </summary>
		public virtual void ComputeWeights()
		{
			if (ObjectsToLoot == null)
			{
				return;
			}

			if (ObjectsToLoot.Count == 0)
			{
				return;
			}

			_maximumWeightSoFar = 0f;

			foreach(T lootDropItem in ObjectsToLoot)
			{
				if(lootDropItem.Weight >= 0f)
				{
					lootDropItem.RangeFrom = _maximumWeightSoFar;
					_maximumWeightSoFar += lootDropItem.Weight;	
					lootDropItem.RangeTo = _maximumWeightSoFar;
				} 
				else 
				{
					lootDropItem.Weight =  0f;						
				}
			}

			WeightsTotal = _maximumWeightSoFar;

			foreach(T lootDropItem in ObjectsToLoot)
			{
				lootDropItem.ChancePercentage = ((lootDropItem.Weight) / WeightsTotal) * 100;
			}

			_weightsComputed = true;
		}
        
		/// <summary>
		/// Returns one object from the table, picked randomly
		/// </summary>
		/// <returns></returns>
		public virtual T GetLoot()
		{	
			if (ObjectsToLoot == null)
			{
				return null;
			}

			if (ObjectsToLoot.Count == 0)
			{
				return null;
			}

			if (!_weightsComputed)
			{
				ComputeWeights();
			}
            
			float index = Random.Range(0, WeightsTotal);
 
			foreach (T lootDropItem in ObjectsToLoot)
			{
				if ((index > lootDropItem.RangeFrom) && (index < lootDropItem.RangeTo))
				{
					return lootDropItem;
				}
			}	
            
			return null;
		}
	}
    
	/// <summary>
	/// A MMLootTable implementation for GameObjects
	/// </summary>
	[System.Serializable]
	public class MMLootTableGameObject : MMLootTable<MMLootGameObject, GameObject> { } 
    
	/// <summary>
	/// A MMLootTable implementation for floats
	/// </summary>
	[System.Serializable]
	public class MMLootTableFloat : MMLootTable<MMLootFloat, float> { } 
    
	/// <summary>
	/// A MMLootTable implementation for strings
	/// </summary>
	[System.Serializable]
	public class MMLootTableString : MMLootTable<MMLootString, string> { } 
}