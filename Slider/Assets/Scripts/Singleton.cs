using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour 
    where T : Singleton<T>
{
    protected static T _instance;

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError($"Multiple Singleton components of type {typeof(T)} were detected. The latest one was deleted.");
            Destroy(this);
        }
        else
        {
            T[] instances = FindObjectsOfType<T>();
            if (instances.Length > 1)
            {
                Debug.LogError($"Multiple Singleton components of type {typeof(T)} were detected. An arbitrary one was chosen " +
                    "to keep. This needs to be fixed immediately to avoid catastrophic consequences. Don't make us come after you. " +
                    "We will not forgive your sins.");
            }
            _instance = instances[0];
            for (int i = 1; i < instances.Length; i++)
            {
                Destroy(instances[i]);
            }
        }
    }
}
