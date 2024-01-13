using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class MirageSTileManager : Singleton<MirageSTileManager>, ISavable
{
    [SerializeField] private List<GameObject> mirageSTiles;
    [SerializeField] private List<ArtifactTBPluginMirage> mirageButtons;
    //public static Vector2Int mirageTailPos;
    public static EventHandler OnMirageSTilesEnabled;
    public List<STileTilemap> MirageMaterialTileMaps;

    private bool mirageEnabled;
    public bool MirageEnabled => mirageEnabled;

    /// <summary>
    /// The scale factor from the position of a tile on the grid to the transform.position of the tile.
    /// </summary>
    private const int GRID_POSITION_TO_WORLD_POSITION = 17;

    private const string MIRAGE_ENABLED_SAVE_STRING = "DesertMirageEnabled";
    private const string MIRAGE_TILES_SAVE_STRING = "DesertMirageTiles";

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

    public void Save()
    {
        if(mirageEnabled)
        {
            UnSubscribeMirageEvents();
        }
        SaveSystem.Current.SetBool(MIRAGE_ENABLED_SAVE_STRING, mirageEnabled);
        SaveSystem.Current.SetString(MIRAGE_TILES_SAVE_STRING, BuildMirageTilesSaveString());
    }

    private string BuildMirageTilesSaveString()
    {
        if(enabledMirageTiles == null) return "";
        StringBuilder s = new();
        if(enabledMirageTiles.Count > 2) 
            Debug.LogWarning("more than 2 mirage tiles saved! This should not happen!");
        for(int i = 1; i <= 7; i++)
        {
            MirageTileData data = null;
            foreach(MirageTileData d in enabledMirageTiles)
            {
                if(d.orignalTileID == i)
                    data = d;
            }   
            if(data == null)
            {
                s.Append("XXXX");
            }
            else
            {
                s.Append($"{data.orignalTileID}{data.buttonID}{data.x}{data.y}");
            }
        }
       // print(s.ToString());
        return s.ToString();
    }

    public void Load(SaveProfile profile)
    {
        mirageEnabled = profile.GetBool(MIRAGE_ENABLED_SAVE_STRING);
        if(mirageEnabled)
        {
            EnableMirage();
        }
        EnableMirageTilesFromSave(profile.GetString(MIRAGE_TILES_SAVE_STRING, ""));
    }

    private void EnableMirageTilesFromSave(string saveString)
    {
        enabledMirageTiles = new();
        print(saveString);
        if(saveString == null || saveString == "") return;
        for(int i = 0; i < 7; i++)
        {
            string s = saveString.Substring(4 * i, 4);
            char[] c = s.ToCharArray();
            if(c[0] != 'X')
            {
                int[] ints = CharToInt(c);
                EnableMirageTile(ints[0], ints[1], ints[2], ints[3]);
                if(ints[1] == 8)
                {
                    mirageButtons[0].EnableMirageButton(ints[0]);
                }
                else
                {
                    mirageButtons[1].EnableMirageButton(ints[0]);
                }
            }
        }
        OnMirageSTilesEnabled?.Invoke(this, null);
    }

    private int[] CharToInt(char[] chars)
    {
        int[] ints = new int[chars.Length];
        for(int i = 0; i < chars.Length; i++)
        {
            ints[i] = chars[i] - '0';
        }
        return ints;
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


    public void EnableMirage()
    {
        mirageEnabled = true;
        SubscibeMirageEvents();
        EnableMirageVFX();
    }

    public void EnableMirageVFX() {}

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

    public void DisableMirageVFX() {}

    public void EnableMirageTile(int islandId, int buttonID, int x, int y, bool addData = true)
    {
        if (islandId > 7 || islandId < 1) return;
        mirageSTiles[islandId - 1].transform.position = new Vector2(x * GRID_POSITION_TO_WORLD_POSITION, y * GRID_POSITION_TO_WORLD_POSITION);
        mirageSTiles[islandId - 1].gameObject.SetActive(true);
        if(addData)
            AddMirageTileData(islandId, buttonID, x, y);
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

            AudioManager.Play("Hurt");
            UIEffects.FadeFromBlack(null, 1.5f);
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
        Vector2 pos = Player.GetInstance().transform.position;
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
        islandId = -1;
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
