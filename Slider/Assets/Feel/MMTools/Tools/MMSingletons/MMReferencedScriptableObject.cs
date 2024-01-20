using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Auto-referenced ScriptableObject instances of type T
	/// An example usage for ReferenceHolder<T> that can be used with any class type
	/// </summary>
	public class MMReferencedScriptableObject<T> : ScriptableObject where T : ScriptableObject
	{
		private MMReferenceHolder<T> _instances;
		protected virtual T Typed => _typed = _typed ?? this as T; private T _typed;

		protected virtual void OnReferenced() {}
		protected virtual void OnEnable()
		{
			_instances.Reference(Typed);
			OnReferenced();
			// Debug.Log(ReferenceHolder<T>.Any != null, this);
		}

		protected virtual void OnDisposed() {}
		protected virtual void OnDisable()
		{
			_instances.Dispose();
			OnDisposed();
			// Debug.Log(ReferenceHolder<T>.Any != null);
		}
	}

	// using WeakReference to let GC collect those once Engine does not use them anymore
	public struct MMReferenceHolder<T> : IDisposable where T : class
	{
		private static List<WeakReference<T>> _instances = new List<WeakReference<T>>(2);

		private WeakReference<T> _instance;
		public void Reference(T instance, bool cleanUp = false)
		{
			_instances = _instances ?? new List<WeakReference<T>>(1);
			if(cleanUp) CleanUp();
			if (instance != null)
			{
				_instance = new WeakReference<T>(instance);
				_instances.Add(_instance); // always adding at the end, to keep it cheap, do a CleanUp if needed
			}
		}
		public void Dispose()
		{
			if (_instance != null) _instances?.Remove(_instance);
		}

		public static void CleanUp() => RepackNonNullReferences();
		static void RepackNonNullReferences()
		{
			if (_instances == null) return;
			for(int n=_instances.Count-1; n >=0; --n)
			{
				if (!_instances[n].TryGetTarget(out T target))
				{
					_instances.RemoveAt(n);
				}
			}
		}

		public static T Any => _instances != null && _instances.Count > 0 && _instances[0].TryGetTarget(out T target) ? target : null;
		public static IEnumerator<T> All
		{
			get
			{
				if (_instances == null) yield break;
				foreach (var inst in _instances)
				{
					if (inst.TryGetTarget(out T target))
					{
						yield return target;
					}
				}
			}
		}
		
		public static T First(System.Func<T,bool> selector)
		{
			if (_instances == null) return null;
			if (selector == null) return Any;
			foreach (var inst in _instances)
			{
				if (inst.TryGetTarget(out T target) && selector(target))
				{
					return target;
				}
			}
			return null;
		}
	}
}