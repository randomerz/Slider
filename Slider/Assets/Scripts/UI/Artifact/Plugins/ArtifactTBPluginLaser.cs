using System;
using System.Collections.Generic;
using UnityEngine;    

public class ArtifactTBPluginLaser : ArtifactTBPlugin
{
    public GameObject[] sprites; //0 = East, 1 = north, 2 = west, 3 = south
    public GameObject[] emptysprites; //0 = East, 1 = north, 2 = west, 3 = south
    public bool[] edgeblockers;
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

    public static Dictionary<ArtifactTileButton, ArtifactTBPluginLaser> tileDict;
    public static ArtifactTBPluginLaser source;

    private int MAX_CROSSINGS = 12;
    private int crossings = 0;

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
            laserUI.t5RockBS = false;
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

    public List<LaserableRockUIData> rockdata;
    public bool t5RockBS;
    public GameObject[] t5Sprites;
    public LaserUIOffMap laserUIOffMap;
 

    private void Awake()
    {
        button = GetComponentInParent<ArtifactTileButton>();
        button.plugins.Add(this);
        if(tileDict == null)
            tileDict = new();
        tileDict.Add(button, this);
        ResetSprites();
        if(centerObject == LaserCenterObject.SOURCE)
            source = this;
    }

    private void OnEnable()
    {
        UpdateSpritesFromSource();
    }

    private void OnDestroy()
    {
        tileDict.Clear();
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
            if(s!= null)
                s.SetActive(false);
        }
        foreach(GameObject s in emptysprites)
        {
            if(s!= null)
                s.SetActive(false);
        }
        if(t5RockBS)
        {
            foreach(GameObject s in t5Sprites)
            {
                if(s!= null)
                    s.SetActive(false);
            }
        }
        crossings = 0;
    }

    public void UpdateEdgeToCenter(int direction)
    {
        if(button.TileIsActive)
        {
            if(t5RockBS && direction == 1)
                t5Sprites[0].SetActive(true);
            else
                sprites[direction].SetActive(true);
            if(edgeblockers[direction])
                return;
        }
        else
            emptysprites[direction].SetActive(true);

        crossings++;
        UpdateCenter(direction);
    }

    public void UpdateCenter(int direction)
    {
        crossings++;
        int nextDir = (direction + 2) % 4;

        if(!button.TileIsActive)
        {
            UpdateCenterToEdge(nextDir);
            return;
        }

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
        crossings++;

        if(button != null && button.TileIsActive)
        {
            if(t5RockBS && direction == 1)
                t5Sprites[1].SetActive(true);

            if(edgeblockers[direction])
                return;
                
            else if(!t5RockBS || direction != 1)
                sprites[direction].SetActive(true);
        }
        else
            emptysprites[direction].SetActive(true);

        if(crossings > MAX_CROSSINGS)
        {
            Debug.LogError("Laser UI in infinite loop. Terminated to prevent stackoverflow");
            return;
        }
        UpdateAdjTile(direction);
    }

    public static Vector2Int GetTileOffsetVector(int num)
    {
        int[] arr = {1, 0, -1, 0};
        return new Vector2Int(arr[num], arr[(num+3) % 4]);
    }

    public void UpdateAdjTile(int direction)
    {
        if(button == null) return;
        int nextX = button.x + GetTileOffsetVector(direction)[0];
        int nextY = button.y + GetTileOffsetVector(direction)[1];

        if(button.y == 2 && button.x == 3 && nextX == 2)
        {
            laserUIOffMap.ShowLaser();
        }
        
        if(nextX < 0 || nextY < 0 || nextX > 5 || nextY > 2) return;
        if(button.x > 2 != nextX > 2) return;
        
        int nextDir = (direction + 2) % 4;
        foreach(ArtifactTileButton a in tileDict.Keys)
        {
            if(a.x == nextX && a.y == nextY)
            {
                tileDict[a].UpdateEdgeToCenter(nextDir);
            }
        }
    }


    public override void OnPosChanged()
    {
        UpdateSpritesFromSource();
    }

    public static void UpdateSpritesFromSource()
    {
        if(source == null) return;
        source.UpdateSprites();
    }

    public void UpdateSprites()
    {
        if(tileDict == null) return;
        laserUIOffMap.HideLaser();
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