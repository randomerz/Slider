using UnityEngine;

public class MonoBehaviourContextProvider<T> where T : MonoBehaviour
{
    protected T context;

    public MonoBehaviourContextProvider(T context)
    {
        this.context = context;
    }

    public virtual void Awake() { }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }

    public virtual void Start() { }
    public virtual void Update() { }
}

