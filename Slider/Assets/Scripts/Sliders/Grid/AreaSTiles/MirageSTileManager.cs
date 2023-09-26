using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MirageSTileManager : Singleton<MirageSTileManager>
{

    [SerializeField] private List<GameObject> mirageSTiles;
    public static Vector2Int mirageTailPos;

    public void Awake()
    {
        InitializeSingleton();
        mirageTailPos = new Vector2Int(-1, -1);
    }

    public void EnableMirage(int islandId, int x, int y)
    {
        if (islandId > 7) return;
        //Do some STile collider crap
        mirageSTiles[islandId - 1].transform.position = new Vector2(x*17, y*17);
        mirageSTiles[islandId - 1].gameObject.SetActive(true);
        if (islandId == 7) mirageTailPos = new Vector2Int(x, y);
        Debug.Log(mirageTailPos);
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
        //if (isPlayerOnMirage(out mirageIsland))
        //{
        //    Debug.Log($"Player on Mirage! Current mirage: {mirageIsland}");
        //    Player.GetInstance().transform.SetParent(grid.GetStile(mirageIsland).transform, false);
        //}
        //Insert disable effect
        if (islandId == 0 || islandId > 7) return;
        if (islandId < 0) foreach (GameObject o in mirageSTiles) o.SetActive(false);
        if (islandId == 7) mirageTailPos = new Vector2Int(-1, -1);
        else mirageSTiles[islandId - 1].gameObject.SetActive(false);
        Debug.Log(mirageTailPos);
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
