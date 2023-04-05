using System.Collections.Generic;

using UnityEngine;

public abstract class MonoBehaviourContextSubscriber<T> : MonoBehaviour where T : MonoBehaviour
{
    private List<MonoBehaviourContextProvider<T>> providers;
    protected void RegisterProvider(MonoBehaviourContextProvider<T> provider)
    {
        if (providers == null)
        {
            providers = new List<MonoBehaviourContextProvider<T>>();
        }
        providers.Add(provider);
    }
    
    protected void Awake()
    {
        InitializeContexts();
        providers.ForEach((provider) => provider.Awake());
    }

    protected void OnEnable()
    {
        providers.ForEach((provider) => provider.OnEnable());
    }

    protected void OnDisable()
    {
        providers.ForEach((provider) => provider.OnDisable());
    }

    protected void Start()
    {
        providers.ForEach((provider) => provider.Start());
    }

    protected void Update()
    {
        providers.ForEach((provider) => provider.Update());
    }

    protected abstract void InitializeContexts();
}