using UnityEngine;    

//L: This is a way to inject more implementation into a button without using inheritance (since swapping components in Unity doesn't save serialized values).
public abstract class ArtifactTBPlugin : MonoBehaviour
{
    [SerializeField]
    protected ArtifactTileButton button;

    public virtual void Init(ArtifactTileButton button) { } // Called by ArtifactTileButton.Init() because Awake is not reliable for artifact ui
    public virtual void OnPosChanged() { }
}