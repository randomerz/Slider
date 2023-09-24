using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MirageSTileManager : Singleton<MirageSTileManager>
{

    [SerializeField] private List<GameObject> mirageSTiles;
    private int fragMirage;

    public void Awake()
    {
        InitializeSingleton();
        fragMirage = 0;
    }

    public void EnableMirage(int islandId, int x, int y, int buttonId)
    {
        Debug.Log($"enabled: {islandId}");
        if (islandId > 7) return;
        //Do some STile collider crap
        mirageSTiles[islandId - 1].transform.position = new Vector2(x*17, y*17);
        mirageSTiles[islandId - 1].gameObject.SetActive(true);
        if (buttonId == 9) fragMirage = islandId;
        //Insert enabling coroutine fading in
    }
    /// <summary>
    /// Function that disables mirages either from selecting or from making an artifact move
    /// </summary>
    /// <param name="islandId">0 means disable all mirages</param>
    public void DisableMirage(int buttonId, int islandId = -1)
    {
        Debug.Log($"disable called with: {islandId}");
        //Do player location check and random parenting bs
        int mirageIsland;
        //if (isPlayerOnMirage(out mirageIsland))
        //{
        //    Debug.Log($"Player on Mirage! Current mirage: {mirageIsland}");
        //    Player.GetInstance().transform.SetParent(grid.GetStile(mirageIsland).transform, false);
        //}
        //Insert disable effect
        if (buttonId == 9) fragMirage = 0;
        if (islandId == 0 || islandId > 7) return;
        if (islandId < 0) foreach (GameObject o in mirageSTiles) o.SetActive(false);
        else mirageSTiles[islandId - 1].gameObject.SetActive(false);
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

    public void UpdateMirageSTileOnFrag(int x, int y)
    {
        if (fragMirage < 1) return;
        mirageSTiles[fragMirage - 1].transform.position = new Vector2(x * 17, y * 17);
    }

    public static MirageSTileManager GetInstance()
    {
        return _instance;
    }
}
