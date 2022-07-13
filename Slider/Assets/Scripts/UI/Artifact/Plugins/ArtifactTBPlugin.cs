using UnityEngine;    

//L: This is a way to inject more implementation into a button without using inheritance (since swapping components in Unity doesn't save serialized values).
public abstract class ArtifactTBPlugin : MonoBehaviour
{
    [SerializeField]
    protected ArtifactTileButton button;

    public virtual void OnPosChanged() { }
}