using UnityEngine;

public class ArtifactWorldMapAffector : MonoBehaviour 
{
    public Area area;
    public ArtifactWorldMapArea.AreaStatus status;

    public void UpdateStatus()
    {
        UIArtifactWorldMap.SetAreaStatus(area, status);
    }
}