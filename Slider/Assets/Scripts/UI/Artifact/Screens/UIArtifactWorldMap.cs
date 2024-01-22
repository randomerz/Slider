using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactWorldMap : Singleton<UIArtifactWorldMap>, ISavable
{
    public List<ArtifactWorldMapArea> mapAreas;
    // Make sure to have them in order of enums!
    
    private Dictionary<Area, ArtifactWorldMapArea> areaToMapArea = new Dictionary<Area, ArtifactWorldMapArea>();
    private Dictionary<Area, ArtifactWorldMapArea.AreaStatus> areaToStatus;
    public GameObject inventoryText;
    public Image controllerSelectImage;
    public Sprite empty;

    private bool didInit = false;

    public static UIArtifactWorldMap GetInstance()
    {
        return _instance;
    }

    private void Awake() 
    {
        Init();
    }

    public void Init()
    {
        if (didInit)
            return;
        didInit = true;
        
        InitializeSingleton(this);
    }

    public void Save()
    {
        Dictionary<Area, SGridData> areaToSgridData = SaveSystem.Current.GetAreaToSGridData();

        foreach (Area area in Area.GetValues(typeof(Area)))
        {
            if (area == Area.None)
                continue; // 0 is Area.none

            areaToSgridData[area].completionColor = areaToStatus[area];
        }

        SaveSystem.Current.SetAreaToSGridData(areaToSgridData);
    }

    public void Load(SaveProfile profile)
    {
        foreach (int i in Area.GetValues(typeof(Area)))
        {
            if (i == 0)
                continue; // 0 is Area.none

            // Debug.Log(i);
            areaToMapArea[(Area)i] = mapAreas[i - 1]; // offset 0
        }

        LoadAreaToStatus(profile);

        foreach (int i in Area.GetValues(typeof(Area)))
        {
            if (i == 0)
                continue; // 0 is Area.none

            areaToMapArea[(Area)i].SetStatus(areaToStatus[(Area)i]);
            areaToMapArea[(Area)i].UpdateSprite(true);
        }
    }

    private void LoadAreaToStatus(SaveProfile profile)
    {
        areaToStatus = new Dictionary<Area, ArtifactWorldMapArea.AreaStatus>();

        foreach (Area area in Area.GetValues(typeof(Area)))
        {
            if (area == Area.None)
                continue; // 0 is Area.none

            areaToStatus[area] = profile.GetSGridData(area).completionColor;
        }
    }

    public void Start()
    {
        ToggleNavigation(Controls.CurrentControlScheme);
    }

    // this is bc we refactored this to be a singleton
    public static void SetAreaStatus(Area area, ArtifactWorldMapArea.AreaStatus status)
    {
        _instance._SetAreaStatus(area, status);
    }

    public void _SetAreaStatus(Area area, ArtifactWorldMapArea.AreaStatus status)
    {
        if (areaToMapArea[area].SetStatus(status))
        {
            areaToStatus[area] = status;
        }
    }

    // For inspectors/NPCs
    public void SetAreaStatusSilhouette(string areaName)
    {
        Area area = Area.None;
        if (!Area.TryParse<Area>(areaName, out area))
        {
            Debug.LogError("Area was not able to be parsed. Did you type it correctly?");
            return;
        }
        _instance._SetAreaStatus(Area.Parse<Area>(areaName), ArtifactWorldMapArea.AreaStatus.silhouette);
    }

    public ArtifactWorldMapArea.AreaStatus GetAreaStatus(Area area)
    {
        return areaToStatus[area];
    }

    public void ClearAreaStatus(Area area)
    {
        areaToMapArea[area].ClearStatus();
        areaToStatus[area] = ArtifactWorldMapArea.AreaStatus.none;
    }

    public void UpdateAreaStatuses()
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

    [SerializeField] private Button leftArrowButton;

    private void OnEnable()
    {
        if (Player.GetInstance().GetCurrentControlScheme() == "Controller")
        {
            leftArrowButton.Select();
        }
        Player.OnControlSchemeChanged += ToggleNavigation;
    }

    private void OnDisable()
    {
        Player.OnControlSchemeChanged -= ToggleNavigation;
    }

    private void ToggleNavigation(string s)
    {
        if(s == Controls.CONTROL_SCHEME_CONTROLLER)
        {
            foreach(ArtifactWorldMapArea a in mapAreas)
            {
                a.ToggleControllerSelect(true);
                a.SelectCurrentArea(s);
            }
        }
        else
        {
            foreach(ArtifactWorldMapArea a in mapAreas)
            {
                a.ToggleControllerSelect(false);
            }
            ClearControllerSelectionSprite();
        }
        
    }

    public void UpdateControllerSelectionSprite(Sprite s)
    {
        controllerSelectImage.sprite = s;
    }

    public void ClearControllerSelectionSprite()
    {
        controllerSelectImage.sprite = empty;
    }
}
