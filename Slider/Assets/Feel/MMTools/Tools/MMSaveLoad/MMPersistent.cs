using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A persistent class that can save the essential parts of an object :
	/// its transform data (position, rotation, scale) and its active state
	/// This inherits from MMPersistentBase and implements the IMMPersistent interface
	/// It's a good example of how to implement the interface's OnSave and OnLoad methods
	/// </summary>
	public class MMPersistent : MMPersistentBase
	{
		
		[Header("Properties")]
		/// whether or not to save this object's position
		[Tooltip("whether or not to save this object's position")]
		public bool SavePosition = true;
		/// whether or not to save this object's rotation
		[Tooltip("whether or not to save this object's rotation")]
		public bool SaveLocalRotation = true;
		/// whether or not to save this object's scale
		[Tooltip("whether or not to save this object's scale")]
		public bool SaveLocalScale = true;
		/// whether or not to save this object's active state
		[Tooltip("whether or not to save this object's active state")]
		public bool SaveActiveState = true;
		/// whether or not to save this object's components' enabled states
		[Tooltip("whether or not to save this object's components' enabled states")]
		public bool SaveEnabledStates = false;
		
		/// <summary>
		/// A struct used to store and serialize the data we want to save
		/// </summary>
		[Serializable]
		public struct Data 
		{
			public Vector3 Position;
			public Quaternion LocalRotation;
			public Vector3 LocalScale;
			public bool ActiveState;
			public List<ComponentData> ComponentEnabledStates;
		}

		[Serializable]
		public struct ComponentData
		{
			public string Name;
			public bool EnabledState;
		}

		/// <summary>
		/// On Save, we turn the object's transform data and active state to a Json string and return it to the MMPersistencyManager
		/// </summary>
		/// <returns></returns>
		public override string OnSave()
		{
			List<ComponentData> saveEnabledStates = null;
			if (SaveEnabledStates)
			{
				saveEnabledStates = GetCurrentComponents();
			}
			return JsonUtility.ToJson(new Data { Position = this.transform.position, 
													LocalRotation = this.transform.localRotation, 
													LocalScale = this.transform.localScale, 
													ActiveState = this.gameObject.activeSelf,
													ComponentEnabledStates = saveEnabledStates
			});
		}

		/// <summary>
		/// On load, we read the saved json data and apply it to our object's properties
		/// </summary>
		/// <param name="data"></param>
		public override void OnLoad(string data)
		{
			if (SavePosition)
			{
				this.transform.position = JsonUtility.FromJson<Data>(data).Position;
			}

			if (SaveLocalRotation)
			{
				this.transform.localRotation = JsonUtility.FromJson<Data>(data).LocalRotation;	
			}

			if (SaveLocalScale)
			{
				this.transform.localScale = JsonUtility.FromJson<Data>(data).LocalScale;	
			}

			if (SaveActiveState)
			{
				this.gameObject.SetActive(JsonUtility.FromJson<Data>(data).ActiveState);	
			}

			if (SaveEnabledStates)
			{
				List<ComponentData> loadedList = JsonUtility.FromJson<Data>(data).ComponentEnabledStates;
				
				Behaviour[] components = gameObject.GetComponents<Behaviour>();
				Renderer[] renderers = gameObject.GetComponents<Renderer>();

				if (loadedList.Count != components.Length + renderers.Length)
				{
					return;
				}

				int total = 0;
				for (int i = 0; i < components.Length; i++)
				{
					if (loadedList[i].Name == components[i].name)
					{
						components[i].enabled = loadedList[i].EnabledState;
					}

					total++;
				}
				
				for (int j = 0; j < renderers.Length; j++)
				{
					if (loadedList[total+j].Name == renderers[j].name)
					{
						renderers[j].enabled = loadedList[total+j].EnabledState;
					}
				}
			}
		}

		/// <summary>
		/// Grabs all components on this object
		/// </summary>
		protected virtual List<ComponentData> GetCurrentComponents()
		{
			List<ComponentData> currentComponents = new List<ComponentData>();
			Behaviour[] components = gameObject.GetComponents<Behaviour>();
			Renderer[] renderers = gameObject.GetComponents<Renderer>();
			foreach (Behaviour component in components) 
			{ 
				currentComponents.Add(new ComponentData { Name = component.name, EnabledState = component.enabled }); 
			}
			foreach (Renderer renderer in renderers) 
			{ 
				currentComponents.Add(new ComponentData { Name = renderer.name, EnabledState = renderer.enabled }); 
			}

			return currentComponents;
		}
	}	
}

