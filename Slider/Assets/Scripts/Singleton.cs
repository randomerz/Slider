using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Singleton<T> : MonoBehaviour 
    where T : Singleton<T>
{
    protected static T _instance;

    /// <summary>
    /// This should be called in Awake inside of all singleton components. This sets up _instance to be the component instance,
    /// enforces singleton-ness, and logs an error if a duplicate of the component is detected.
    /// <br></br>
    /// ** We have an allowActiveDuplicates option only as a temporary measure until everything is properly fixed. DO NOT USE THIS UNLESS YOU HAVE TO. **
    /// </summary>
    /// <param name="allowInactiveDuplicates">If this is true, inactive duplicates of this singleton component will not be deleted, 
    /// nor will errors be logged because of them</param>
    protected virtual void InitializeSingleton(bool allowInactiveDuplicates = false, bool allowActiveDuplicates = false)
    {
        if (allowActiveDuplicates)
        {
            return;
        }
        if (_instance != null)
        {
            Debug.LogError($"Multiple Singleton components of type {typeof(T)} were detected. The latest one was deleted.");
            Destroy(this);
        }
        else
        {
            IEnumerable<T> instances = FindObjectsOfType<T>();
            if (allowInactiveDuplicates)
            {
                instances = instances.Where(instance => instance.isActiveAndEnabled);
            }
            if (instances.Count() > 1)
            {
                Debug.LogError($"Multiple Singleton components of type {typeof(T)} were detected. An arbitrary one was chosen " +
                    "to keep. This needs to be fixed immediately to avoid catastrophic consequences. Don't make us come after you. " +
                    "We will not forgive your sins.");
            }
            _instance = instances.First();
            instances.ToList().ForEach(instance => Destroy(instance));
            /*for (int i = 1; i < instances.Count; i++)
            {
                Destroy(instances[i]);
            }*/
        }
    }
}
