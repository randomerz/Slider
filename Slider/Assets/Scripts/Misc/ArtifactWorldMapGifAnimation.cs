using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactWorldMapGifAnimation : MonoBehaviour
{
    private Area[] areas = new Area[] {
        Area.Village,
        Area.Caves,
        Area.Ocean,
        Area.Mountain,
        Area.Jungle,
        Area.Military,
        Area.Desert,
        Area.Factory,
        Area.MagiTech,
    };

    public void ClearAllAreas()
    {
        foreach (Area a in areas)
            UIArtifactWorldMap.GetInstance().ClearAreaStatus(a);
    }

    public IEnumerator AnimateAllAreas()
    {

        // foreach (Area a in areas)
        // {
        //     // Debug.Log(a);
        //     StartCoroutine(AnimateArea(a));
        //     yield return new WaitForSeconds(0.1f);
        // }


        StartCoroutine(AnimateArea(Area.Village));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(AnimateArea(Area.Caves));
        StartCoroutine(AnimateArea(Area.Ocean));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(AnimateArea(Area.Mountain));
        StartCoroutine(AnimateArea(Area.Jungle));
        StartCoroutine(AnimateArea(Area.Military));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(AnimateArea(Area.Desert));
        StartCoroutine(AnimateArea(Area.Factory));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(AnimateArea(Area.MagiTech));
    }

    private IEnumerator AnimateArea(Area area)
    {
        UIArtifactWorldMap.SetAreaStatus(area, ArtifactWorldMapArea.AreaStatus.silhouette);

        yield return new WaitForSeconds(0.4f);

        UIArtifactWorldMap.SetAreaStatus(area, ArtifactWorldMapArea.AreaStatus.oneBit);

        yield return new WaitForSeconds(0.4f);
        
        UIArtifactWorldMap.SetAreaStatus(area, ArtifactWorldMapArea.AreaStatus.color);
    }
}
