using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Rendering;
public class MirageSTileManager : Singleton<MirageSTileManager>, ISavable
{
    public static EventHandler OnMirageSTilesEnabled; // Also invokes on stile move end late

    private bool mirageEnabled;
    public bool MirageEnabled => mirageEnabled;
    
    [SerializeField] private List<STileTilemap> MirageMaterialTileMaps;
    [SerializeField] private List<GameObject> mirageSTiles;
    [SerializeField] private List<ArtifactTBPluginMirage> mirageButtons;

    /// <summary>
    /// The scale factor from the position of a tile on the grid to the transform.position of the tile.
    /// </summary>
    private const int GRID_POSITION_TO_WORLD_POSITION = 17;

    public const string MIRAGE_ENABLED_SAVE_STRING = "DesertMirageEnabled";
    private const string MIRAGE_TILES_SAVE_STRING = "DesertMirageTiles";
    private readonly List<int> POSSIBLE_MIRAGE_TILES = new() { 8, 9 };

    private List<MirageTileData> enabledMirageTiles = new();
    private class MirageTileData
    {
        public int orignalTileID;
        public int buttonID;
        public int x;
        public int y;

        public MirageTileData(int orignalTileID, int buttonID, int x, int y)
        {
            this.orignalTileID = orignalTileID;
            this.buttonID = buttonID;
            this.x = x;
            this.y = y;
        }
    }

    public Volume volume;

    public void Save()
    {
        SaveSystem.Current.SetBool(MIRAGE_ENABLED_SAVE_STRING, mirageEnabled);
        BuildMirageTileSaveStrings();
    }

    private string BuildMirageTilesSaveString(int buttonId, string parameter) => $"{MIRAGE_TILES_SAVE_STRING}_{buttonId}_{parameter}";

    private void BuildMirageTileSaveStrings()
    {
        // Clear out old data
        foreach (int buttonID in POSSIBLE_MIRAGE_TILES)
        {
            SaveSystem.Current.SetInt(BuildMirageTilesSaveString(buttonID, "originalTileId"), -1);
        }
        
        foreach(MirageTileData data in enabledMirageTiles)
        {
            SaveSystem.Current.SetInt(BuildMirageTilesSaveString(data.buttonID, "originalTileId"), data.orignalTileID);
            SaveSystem.Current.SetInt(BuildMirageTilesSaveString(data.buttonID, "xPosition"), data.x);
            SaveSystem.Current.SetInt(BuildMirageTilesSaveString(data.buttonID, "yPosition"), data.y);
        }
    }

    public void Load(SaveProfile profile)
    {
        mirageEnabled = profile.GetBool(MIRAGE_ENABLED_SAVE_STRING);
        if (mirageEnabled)
        {
            EnableMirage(true);
        }
        EnableMirageTilesFromSave(profile);
    }

    private void EnableMirageTilesFromSave(SaveProfile profile)
    {
        foreach (int buttonID in POSSIBLE_MIRAGE_TILES)
        {
            int originalID = profile.GetInt(BuildMirageTilesSaveString(buttonID, "originalTileId"), -1);
            int x = profile.GetInt(BuildMirageTilesSaveString(buttonID, "xPosition"));
            int y = profile.GetInt(BuildMirageTilesSaveString(buttonID, "yPosition"));
            if(originalID != -1)
            {
                EnableMirageTile(originalID, buttonID, x, y);
            }
        }
    }

    public void Awake()
    {
        InitializeSingleton();
    }

    private void SubscibeMirageEvents()
    {
        SGridAnimator.OnSTileMoveStart += RemovePlayerOnMirageSTile;
        SGridAnimator.OnSTileMoveEndLate += EnableMirageTilesAfterSMove;
    }

    private void UnSubscribeMirageEvents()
    {
        SGridAnimator.OnSTileMoveStart -= RemovePlayerOnMirageSTile;
        SGridAnimator.OnSTileMoveEndLate -= EnableMirageTilesAfterSMove;
    }
    
    private void Start()
    {
        EnableButtonsOnStart();
    }

    private void OnDisable()
    {
        UnSubscribeMirageEvents();
    }

    private void EnableButtonsOnStart()
    {
        foreach(MirageTileData d in enabledMirageTiles)
        {
            EnableMirageTile(d.orignalTileID, d.buttonID, d.x, d.y, false);
            if(d.buttonID == 8)
            {
                mirageButtons[0].EnableMirageButton(d.orignalTileID);
            }
            else
            {
                mirageButtons[1].EnableMirageButton(d.orignalTileID);
            }
        }  
    }

    public void EnableMirage () => EnableMirage(false);

    public void EnableMirage(bool fromSave)
    {
        mirageEnabled = true;
        SubscibeMirageEvents();
        EnableMirageVFX(fromSave);
    }

    public void EnableMirageVFX(bool fromSave) 
    {
        if(fromSave)
        {
            volume.weight = 1;
        }
        else
        StartCoroutine(MirageVFXCoroutine(0, 1));

    }

    public void DisableMirage()
    {
        mirageEnabled = false;
        UnSubscribeMirageEvents();
        RemovePlayerOnMirageSTile();
        DisableMirageVFX();
        foreach(ArtifactTBPluginMirage button in mirageButtons)
        {
            button.DisableMirageButton();
        }
        DisableMirageTile(-1);
    }

    public void DisableMirageVFX() 
    {
        StartCoroutine(MirageVFXCoroutine(1, 0));
    }

    private IEnumerator MirageVFXCoroutine(float start, float end)
    {
        float duration = 1;
        float t = 0;
        while(t < duration)
        {
            volume.weight = Mathf.Lerp(start, end, t/duration);
            t += Time.deltaTime;
            yield return null;
        }
        volume.weight = end;
    }

    public void EnableMirageTile(int islandId, int buttonID, int x, int y, bool addData = true)
    {
        if (islandId > 7 || islandId < 1) return;
        mirageSTiles[islandId - 1].transform.position = new Vector2(x * GRID_POSITION_TO_WORLD_POSITION, y * GRID_POSITION_TO_WORLD_POSITION);
        mirageSTiles[islandId - 1].gameObject.SetActive(true);
        if(addData)
            AddMirageTileData(islandId, buttonID, x, y);
    }

    public GameObject GetMirageTileForUI(int islandId)
    {
        return mirageSTiles[islandId - 1];
    }

    public int GetButtonIslandID(int mirageID)
    {
        foreach(MirageTileData d in enabledMirageTiles)
        {
            if(d.orignalTileID == mirageID)
                return d.buttonID;
        }
        return -1;
    }

    private void AddMirageTileData(int islandId, int buttonIslandId, int x, int y)
    {
        MirageTileData remove = null;
        foreach(MirageTileData data in enabledMirageTiles)
        {
            if(data.buttonID == buttonIslandId)
                remove = data;
        }
        if(remove != null)
            enabledMirageTiles.Remove(remove);
        enabledMirageTiles.Add(new(islandId, buttonIslandId, x, y));
    }
    
    /// <summary>
    /// Function that disables mirages either from selecting or from making an artifact move
    /// </summary>
    /// <param name="islandId">0 means disable all mirages</param>
    public void DisableMirageTile(int islandId = -1)
    {
        //Insert disable effect
        RemoveMirageData(islandId);
        if (islandId == 0 || islandId > 7) return;
        if (islandId < 0) foreach (GameObject o in mirageSTiles) o.SetActive(false);
        else mirageSTiles[islandId - 1].gameObject.SetActive(false);
    }

    private void RemoveMirageData(int islandId)
    {
        MirageTileData d = null;
        foreach(MirageTileData data in enabledMirageTiles)
        {
            if(data.orignalTileID == islandId)
                d = data;
        }
        if(d == null) return;
        enabledMirageTiles.Remove(d);
    }

    private void EnableMirageTilesAfterSMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!mirageEnabled) return; 
        if (UIArtifact.GetInstance().MoveQueueEmpty())
        {
            //No new moves should be queued before mirage tiles are enabled
            foreach (ArtifactTBPluginMirage button in mirageButtons)
            {
                var buttonBase = button.GetComponent<ArtifactTileButton>();
                EnableMirageTile(button.mirageIslandId, button.buttonIslandId, buttonBase.x, buttonBase.y);
            }

        }
        OnMirageSTilesEnabled?.Invoke(this, null);
    }

    private void RemovePlayerOnMirageSTile(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        RemovePlayerOnMirageSTile();
    }

    private void RemovePlayerOnMirageSTile()
    {
        int mirageIsland;
        bool playerOnMirage = IsPlayerOnMirage(out mirageIsland);

        if (playerOnMirage)
        {
            STile realSTile = DesertGrid.Current.GetStile(mirageIsland);
            Vector3 relativePos = Player._instance.transform.position - mirageSTiles[mirageIsland - 1].transform.position;
            realSTile.SetBorderColliders(false);
            Player.SetParent(realSTile.transform);
            Player.SetPosition(realSTile.transform.position + relativePos);

            AudioManager.Play("Portal");
            UIEffects.FadeFromScreenshot(type: UIEffects.ScreenshotEffectType.MIRAGE);
        }
    }

    /// <summary>
    /// Returns a dictionary of each grid position, represented by Vector2Int ,
    /// to the ID of the mirage tile currently active on that grid position.
    /// If no mirage tile is active on that position, that position does not
    /// appear in the dictionary.
    /// </summary>
    public static Dictionary<Vector2Int, char> GetActiveMirageTileIdsByPosition()
    {
        List<GameObject> mirageTiles = _instance.mirageSTiles;

        Dictionary<Vector2Int, char> tileIdToPosition = new();

        for (int tileId = 0; tileId < mirageTiles.Count; tileId++)
        {
            GameObject mirageTile = mirageTiles[tileId];
            if (mirageTile.activeInHierarchy)
            {
                Vector2Int mirageTileGridPosition = GridPositionFromWorldPosition(mirageTile.transform.position);
                tileIdToPosition[mirageTileGridPosition] = (char)('A' + tileId);
            }
        }

        return tileIdToPosition;
    }

    private static Vector2Int GridPositionFromWorldPosition(Vector2 worldPosition)
    {
        int x = (int)(worldPosition.x / GRID_POSITION_TO_WORLD_POSITION);
        int y = (int)(worldPosition.y / GRID_POSITION_TO_WORLD_POSITION);
        return new Vector2Int(x, y);
    }

    public bool IsPlayerOnMirage(out int islandId)
    {
        return IsObjectOnMirage(Player.GetInstance().transform, out islandId);
    }

    public bool IsObjectOnMirage(Transform t, out int islandId)
    {
        islandId = -1;
        if(!mirageEnabled) return false;

        Vector2 pos = t.position;
        float offset = 8.5f;
        for (int i = 0; i < 7; i++)
        {
            if (!mirageSTiles[i].activeSelf) continue;
            Vector3 stilePos = mirageSTiles[i].transform.position;
            if (stilePos.x - offset < pos.x && pos.x < stilePos.x + offset &&
            (stilePos.y - offset < pos.y && pos.y < stilePos.y + offset))
            {
               islandId = i+1;
               return true;
            }
        }
        return false;
    }

    public static MirageSTileManager GetInstance()
    {
        return _instance;
    }

    public STileTilemap GetMaterialTileMap(int mirageIslandId)
    {
        return MirageMaterialTileMaps[mirageIslandId - 1];
    }

    public void IsMirageEnabled(Condition c)
    {
        c.SetSpec(mirageEnabled);
    }

   
}
