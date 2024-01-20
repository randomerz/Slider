using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A serializable dictionary implementation, as Unity still can't serialize Dictionaries natively
	/// 
	/// How to use :
	///
	/// For each type of dictionary you want to serialize, create a serializable class that inherits from MMSerializableDictionary,
	/// and override the constructor and the SerializationInfo constructor, like so (here with a string/int Dictionary) :
	///
	/// [Serializable]
	/// public class DictionaryStringInt : MMSerializableDictionary<string, int>
	/// {
	///   public DictionaryStringInt() : base() { }
	///   protected DictionaryStringInt(SerializationInfo info, StreamingContext context) : base(info, context) { }
	/// }
	///  
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	[Serializable]
	public class MMSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField] 
		protected List<TKey> _keys = new List<TKey>();
		[SerializeField] 
		protected List<TValue> _values = new List<TValue>();
		
		public MMSerializableDictionary() : base() { }
		public MMSerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
		
		/// <summary>
		/// We save the dictionary to our two lists
		/// </summary>
		public void OnBeforeSerialize()
		{
			_keys.Clear();
			_values.Clear();
			
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				_keys.Add(pair.Key);
				_values.Add(pair.Value);
			}
		}

		/// <summary>
		/// Loads our two lists to our dictionary
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void OnAfterDeserialize()
		{
			this.Clear();

			if (_keys.Count != _values.Count)
			{
				Debug.LogError("MMSerializableDictionary : there are " + _keys.Count + " keys and " + _values.Count + " values after deserialization. Counts need to match, make sure both key and value types are serializable.");
			}

			for (int i = 0; i < _keys.Count; i++)
			{
				this.Add(_keys[i], _values[i]);
			}
		}
	}
}



