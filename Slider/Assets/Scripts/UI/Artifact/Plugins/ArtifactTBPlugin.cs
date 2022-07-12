using UnityEngine;    

//L: This is a way to inject more implementation into a button without using inheritance (since inheritance do
public abstract class ArtifactTBPlugin : MonoBehaviour
{
    [SerializeField]
    protected ArtifactTileButton button;

    public abstract void OnPosChanged();
}