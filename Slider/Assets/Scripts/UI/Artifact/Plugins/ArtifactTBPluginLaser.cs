using System;
using System.Collections.Generic;
using UnityEngine;    

//L: This is a way to inject more implementation into a button without using inheritance (since swapping components in Unity doesn't save serialized values).
public class ArtifactTBPluginLaser : ArtifactTBPlugin
{
    public GameObject[] sprites = new GameObject[4]; //0 = East, 1 = north, 2 = west, 3 = south
    public bool[] edgeblockers = new bool[4];
    public enum LaserCenterObject
    {
        NONE,
        SOURCE,
        PORTAL,
        MIRROR_NWSE,
        MIRROR_NESW,
        BLOCK,
        NOCHANGE,
    }

    public LaserCenterObject centerObject;
    public ArtifactTBPluginLaser otherPortal;
    public MagiLaser laser;

    private int[] mirrorNWSE = {1, 0, 3, 2};
    private int[] mirrorNESW = {3, 2, 1, 0};

    public static Dictionary<ArtifactTileButton, ArtifactTBPluginLaser> tileDict = new();
    public static ArtifactTBPluginLaser source;

    [Serializable]
    public class LaserableRockUIData
    {
        private bool exploded;
        public ExplodableRock rock;
        public ArtifactTBPluginLaser laserUI;
        public int blockLocation;
        public LaserCenterObject centerObject;

        public void UpdateLaserUI()
        {
            if(blockLocation != -1)
                laserUI.edgeblockers[blockLocation] = false;
            else if(centerObject != LaserCenterObject.NOCHANGE)
                laserUI.centerObject = centerObject;
            UpdateSpritesFromSource();
        }

        public void CheckForUpdate()
        {
            if(!exploded && rock.isExploded)
            {
                UpdateLaserUI();
            }
        }
    }

    public List<LaserableRockUIData> rockdata = new();
 

    private void Awake()
    {
        button = GetComponentInParent<ArtifactTileButton>();
        tileDict.Add(button, this);
        ResetSprites();
        if(centerObject == LaserCenterObject.SOURCE)
            source = this;
    }

    private void Update()
    {
        foreach(LaserableRockUIData d in rockdata)
            d.CheckForUpdate();
    }

    public void ResetSprites()
    {
        foreach(GameObject s in sprites)
        {
            s.SetActive(false);
        }
    }

    public void UpdateEdgeToCenter(int direction)
    {
        if(edgeblockers[direction])
            return;
        sprites[direction].SetActive(true);
        UpdateCenter(direction);
    }

    public void UpdateCenter(int direction)
    {
        int nextDir = (direction + 2) % 4;
        switch(centerObject)
        {
            case LaserCenterObject.BLOCK:
                return;
            case LaserCenterObject.MIRROR_NWSE:
                nextDir = mirrorNWSE[direction];
                break;
            case LaserCenterObject.MIRROR_NESW:
                nextDir = mirrorNESW[direction];
                break;
            case LaserCenterObject.PORTAL:
                otherPortal.UpdateCenterToEdge(nextDir);
                return;
            default:
                break;
        }
        UpdateCenterToEdge(nextDir);
    }

    public void UpdateCenterToEdge(int direction)
    {
        sprites[direction].SetActive(true);
        if(edgeblockers[direction])
            return;
        UpdateAdjTile(direction);
    }

    public static Vector2Int GetTileOffsetVector(int num)
    {
        int[] arr = {1, 0, -1, 0};
        return new Vector2Int(arr[num], arr[(num+3) % 4]);
    }

    public void UpdateAdjTile(int direction)
    {
        int nextX = button.x + GetTileOffsetVector(direction)[0];
        int nextY = button.y + GetTileOffsetVector(direction)[1];

        int nextDir = (direction + 2) % 4;
        foreach(ArtifactTileButton a in tileDict.Keys)
        {
            if(a.x == nextX && a.y == nextY)
            {
                tileDict[a].UpdateEdgeToCenter(nextDir);
            }
        }

        //TODO: Go through empty tiles

    }

    public override void OnPosChanged()
    {
        if(centerObject is LaserCenterObject.SOURCE)
        {
            UpdateSprites();
        }
    }

    public static void UpdateSpritesFromSource()
    {
        source.UpdateSprites();
    }

    public void UpdateSprites()
    {
        foreach(ArtifactTBPluginLaser l in tileDict.Values)
        {
            l.ResetSprites();
        }
        if(laser.isEnabled)
        {
            UpdateCenterToEdge(2);
        }
    }
}