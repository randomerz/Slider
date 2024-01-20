using UnityEngine;

namespace  MoreMountains.Tools
{
	/// <summary>
	/// A scriptable object containing a MMLootTable definition for game objects
	/// </summary>
	[CreateAssetMenu(fileName="LootDefinition",menuName="MoreMountains/Loot Definition")]
	public class MMLootTableGameObjectSO : ScriptableObject
	{
		/// the loot table 
		public MMLootTableGameObject LootTable;

		/// returns an object from the loot table
		public virtual GameObject GetLoot()
		{
			return LootTable.GetLoot()?.Loot;
		}
        
		/// <summary>
		/// computes the loot table's weights
		/// </summary>
		public virtual void ComputeWeights()
		{
			LootTable.ComputeWeights();
		}
		
		protected virtual void OnValidate()
		{
			ComputeWeights();
		}
	}
}