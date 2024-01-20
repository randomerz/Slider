using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A serializable class used to store scene data, the key is a string (the scene name), the value is a MMPersistencySceneData
	/// </summary>
	[Serializable]
	public class DictionaryStringSceneData : MMSerializableDictionary<string, MMPersistenceSceneData>
	{
		public DictionaryStringSceneData() : base() { }
		protected DictionaryStringSceneData(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// A serializable class used to store object data, the key is a string (the object name), the value is a string (the object data)
	/// </summary>
	[Serializable]
	public class DictionaryStringString : MMSerializableDictionary<string, string>
	{
		public DictionaryStringString() : base() { }
		protected DictionaryStringString(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
	
	/// <summary>
	/// A serializable class used to store all the data for a persistence manager, a collection of scene datas
	/// </summary>
	[Serializable]
	public class MMPersistenceManagerData
	{
		public string PersistenceID;
		public string SaveDate;
		public DictionaryStringSceneData SceneDatas;
	}
	
	/// <summary>
	/// A serializable class used to store all the data for a scene, a collection of object datas
	/// </summary>
	[Serializable]
	public class MMPersistenceSceneData
	{
		public DictionaryStringString ObjectDatas;
	}
	
	/// <summary>
	/// The various types of persistence events that can be triggered by the MMPersistencyManager
	/// </summary>
	public enum MMPersistenceEventType { DataSavedToMemory, DataLoadedFromMemory, DataSavedFromMemoryToFile, DataLoadedFromFileToMemory }

	/// <summary>
	/// A data structure used to store persistence event data.
	/// To use :
	/// MMPersistencyEvent.Trigger(MMPersistencyEventType.DataLoadedFromFileToMemory, "yourPersistencyID");
	/// </summary>
	public struct MMPersistenceEvent
	{
		public MMPersistenceEventType PersistenceEventType;
		public string PersistenceID;

		public MMPersistenceEvent(MMPersistenceEventType eventType, string persistenceID)
		{
			PersistenceEventType = eventType;
			PersistenceID = persistenceID;
		}

		static MMPersistenceEvent e;
		public static void Trigger(MMPersistenceEventType eventType, string persistencyID)
		{
			e.PersistenceEventType = eventType;
			e.PersistenceID = persistencyID;
		}
	}
}
