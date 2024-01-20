using System;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A base class implementing the IMMPersistent interface, designed to be extended
	/// This mostly takes care of the GUID generation and validation
	/// </summary>
	[AddComponentMenu("")]
	public class MMPersistentBase : MonoBehaviour, IMMPersistent
	{
		[Header("Save")] 
		/// whether or not this object should be saved
		[Tooltip("whether or not this object should be saved")]
		public bool SaveActive = true;

		[Header("ID")]
		/// an optional suffix to add to the GUID, to make it more readable
		[Tooltip("an optional suffix to add to the GUID, to make it more readable")]
		public string UniqueIDSuffix;
		/// the object's unique ID
		[Tooltip("the object's unique ID")]
		[SerializeField]
		[MMReadOnly]
		protected string _guid;
		
		/// a debug button used to force a new GUI generation
		[MMInspectorButton("GenerateGuid")]
		public bool GenerateGuidButton;
		
		/// <summary>
		/// On validate, we make sure the object gets a valid GUID
		/// </summary>
		protected virtual void OnValidate()
		{
			ValidateGuid();
		}

		/// <summary>
		/// Returns the object's GUID
		/// </summary>
		/// <returns></returns>
		public virtual string GetGuid() => _guid;
		
		/// <summary>
		/// Lets you set the object's GUID
		/// </summary>
		/// <param name="newGUID"></param>
		public virtual void SetGuid(string newGUID) => _guid = newGUID;  

		/// <summary>
		/// On save, does nothing, meant to be extended
		/// </summary>
		/// <returns></returns>
		public virtual string OnSave()
		{
			return string.Empty;
		}

		/// <summary>
		/// On load, does nothing, meant to be extended
		/// </summary>
		/// <param name="data"></param>
		public virtual void OnLoad(string data)
		{
		}

		/// <summary>
		/// Lets the persistence manager know whether or not the object should be saved 
		/// </summary>
		/// <returns></returns>
		public virtual bool ShouldBeSaved()
		{
			return SaveActive;
		}

		/// <summary>
		/// Generates a unique ID for the object, using the scene name, the object name, and a GUID
		/// </summary>
		/// <returns></returns>
		public virtual string GenerateGuid()
		{
			string newGuid = Guid.NewGuid().ToString();
			
			string guid =
				this.gameObject.scene.name
				+ "-"
				+ this.gameObject.name
				+ "-"
				+ newGuid;
			
			if (!string.IsNullOrEmpty(UniqueIDSuffix))
			{
				guid += "-" + UniqueIDSuffix;
			}
			
			this.SetGuid(guid);

			return guid;
		}

		/// <summary>
		/// Checks if the object's ID is unique or not
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public virtual bool GuidIsUnique(string guid)
		{
			return Resources.FindObjectsOfTypeAll<MMPersistentBase>().Count(x => x.GetGuid() == guid) == 1;
		}

		/// <summary>
		/// Validates the object's GUID, and generates a new one if needed, until a unique one is found
		/// </summary>
		public virtual void ValidateGuid()
		{
			if (!this.gameObject.scene.IsValid())
			{
				_guid = string.Empty;
				return;
			}

			int maxCount = 1000;
			int i = 0;
			
			while ( (string.IsNullOrEmpty(_guid) || !GuidIsUnique(_guid) ) && (i < maxCount) )
			{
				GenerateGuid();
				i++;
			}

			if (i == maxCount)
			{
				Debug.LogWarning(this.gameObject.name + " couldn't generate a unique GUID after " + maxCount + " tries, you should probably change its UniqueIDSuffix");
			}
		}
	}	
}

