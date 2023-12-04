using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MirageSTileManager : Singleton<MirageSTileManager>
{

    [SerializeField] private List<GameObject> mirageSTiles;
    public static Vector2Int mirageTailPos;

    /// <summary>
    /// The scale factor from the position of a tile on the grid to the transform.position of the tile.
    /// </summary>
    private const int GRID_POSITION_TO_WORLD_POSITION = 17;
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public void Awake()
    {
        InitializeSingleton();
        mirageTailPos = new Vector2Int(-1, -1);
    }

    public void EnableMirage(int islandId, int x, int y)
    {
        if (islandId > 7) return;
        //Do some STile collider crap
        mirageSTiles[islandId - 1].transform.position = new Vector2(x * GRID_POSITION_TO_WORLD_POSITION, y * GRID_POSITION_TO_WORLD_POSITION);
        mirageSTiles[islandId - 1].gameObject.SetActive(true);
        if (islandId == 7) mirageTailPos = new Vector2Int(x, y);
        // Debug.Log(mirageTailPos);

        // Debug.Log($"travis: {DesertGrid.GetGridString()}");
        //Insert enabling coroutine fading in
    }
    
    /// <summary>
    /// Function that disables mirages either from selecting or from making an artifact move
    /// </summary>
    /// <param name="islandId">0 means disable all mirages</param>
    public void DisableMirage(int islandId = -1)
    {
        //Do player location check and random parenting bs
        int mirageIsland;
        if (isPlayerOnMirage(out mirageIsland))
        {
            //Debug.Log($"Player on Mirage! Current mirage: {mirageIsland}");
            //Player.GetInstance().transform.SetParent(grid.GetStile(mirageIsland).transform, false);
        }
        //Insert disable effect
        if (islandId == 0 || islandId > 7) return;
        if (islandId < 0) foreach (GameObject o in mirageSTiles) o.SetActive(false);
        if (islandId == 7) mirageTailPos = new Vector2Int(-1, -1);
        else mirageSTiles[islandId - 1].gameObject.SetActive(false);
        Debug.Log(mirageTailPos);
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
                //tileIdToPosition[mirageTileGridPosition] = tileId + 1;
                tileIdToPosition[mirageTileGridPosition] = ALPHABET[tileId];
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

    private bool isPlayerOnMirage(out int islandId)
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
               islandId = i;
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
}
