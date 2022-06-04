using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIArtifactWorldMap : MonoBehaviour
{
    public List<ArtifactWorldMapArea> mapAreas;
    // Make sure to have them in order of enums!
    
    private static Dictionary<Area, ArtifactWorldMapArea> areaToMapArea = new Dictionary<Area, ArtifactWorldMapArea>();
    private static Dictionary<Area, ArtifactWorldMapArea.AreaStatus> areaToStatus; // TODO: serialize
    public GameObject inventoryText;

    private bool didInit = false;

    private void Awake() 
    {
        Init();
    }

    public void Init()
    {
        if (didInit)
            return;
        didInit = true;

        foreach (int i in Area.GetValues(typeof(Area)))
        {
            if (i == 0)
                continue; // 0 is Area.none

            // Debug.Log(i);
            areaToMapArea[(Area)i] = mapAreas[i - 1]; // offset 0
        }

        if (areaToStatus == null)
        {
            LoadAreaToStatus();
        }

        foreach (int i in Area.GetValues(typeof(Area)))
        {
            if (i == 0)
                continue; // 0 is Area.none

            areaToMapArea[(Area)i].SetStatus(areaToStatus[(Area)i]);
            areaToMapArea[(Area)i].UpdateSprite(true);
        }
    }

    private static void LoadAreaToStatus()
    {
        // TODO: fix this
        areaToStatus = new Dictionary<Area, ArtifactWorldMapArea.AreaStatus>();

        foreach (int i in Area.GetValues(typeof(Area)))
        {
            if (i == 0)
                continue; // 0 is Area.none

            areaToStatus[(Area)i] = ArtifactWorldMapArea.AreaStatus.none;
        }
    }

    public static void SetAreaStatus(Area area, ArtifactWorldMapArea.AreaStatus status)
    {
        // if (areaToMapArea == null)
            

        if (areaToMapArea[area].SetStatus(status))
        {
            areaToStatus[area] = status;
        }
    }

    public static void ClearAreaStatus(Area area)
    {
        areaToMapArea[area].ClearStatus();
        areaToStatus[area] = ArtifactWorldMapArea.AreaStatus.none;
    }

    public static void UpdateAreaStatuses()
    {
        foreach (ArtifactWorldMapArea a in areaToMapArea.Values)
        {
            a.UpdateSprite();
        }
    }

    public void UpdateText(string text)
    {
        inventoryText.GetComponentsInChildren<TextMeshProUGUI>()[0].text = text;
        inventoryText.GetComponentsInChildren<TextMeshProUGUI>()[1].text = text;
    }
}
