using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILaserManager : MonoBehaviour
{
    public Dictionary<ArtifactTileButton, LaserUIData> buttonToLaserUI;
    public HashSet<LaserUIData> sources;
    public List<LaserableRockUIData> rockData;

    private void Update()
    {
        foreach (LaserableRockUIData d in rockData)
        {
            if (d.CheckForUpdate())
            {
                UpdateSpritesFromSource();
            }
        }
    }


    public LaserUIData AddData(ArtifactTBPluginLaser pluginLaser, ArtifactTileButton button)
    {
        if (buttonToLaserUI == null)
            buttonToLaserUI = new();
        LaserUIData data = new(pluginLaser, this, button);
        data.Init();
        buttonToLaserUI.Add(button, data);
        if (data.rockdata != null)
            rockData.AddRange(data.rockdata);
        return data;
    }

    public void AddSource(LaserUIData data)
    {
        if (sources == null)
            sources = new();
        sources.Add(data);
        data.UpdateSprites();
    }

    public void RemoveSource(LaserUIData data)
    {
        if (sources == null) return;
        sources.Remove(data);
    }

    public void UpdateSpritesFromSource()
    {
        if (sources == null) return;
        ResetAllSprites();
        foreach (LaserUIData source in sources)
        {
            source.UpdateSprites();
        }
    }

    private void ResetAllSprites()
    {
        if (buttonToLaserUI == null) return;
        foreach (LaserUIData l in buttonToLaserUI.Values)
        {
            l.ResetSprites();
        }
    }

}


public class LaserUIData
{
    public GameObject[] sprites; //0 = East, 1 = north, 2 = west, 3 = south
    public GameObject[] emptysprites; //0 = East, 1 = north, 2 = west, 3 = south
    public bool[] edgeblockers;
    public LaserCenterObject centerObject;
    public ArtifactTBPluginLaser otherPortal;
    public MagiLaser laser;
    public int sourceDir;

    private int[] mirrorNWSE = {1, 0, 3, 2};
    private int[] mirrorNESW = {3, 2, 1, 0};

    private int MAX_CROSSINGS = 40; 
    private int crossings = 0;
    
    private Sprite[] originalSprites = null;
    public int islandId;
    public int originalIslandId;

    public List<LaserableRockUIData> rockdata;
    public bool t5RockBS;
    public GameObject[] t5Sprites;
    public LaserUIOffMap laserUIOffMap;

    private UILaserManager uILaserManager;
    private ArtifactTileButton button;

    public LaserUIData(ArtifactTBPluginLaser pluginLaser, UILaserManager uILaserManager, ArtifactTileButton button)
    {
        sprites = pluginLaser.sprites;
        emptysprites = pluginLaser.emptysprites;
        edgeblockers = pluginLaser.edgeblockers;
        centerObject = pluginLaser.centerObject;
        otherPortal = pluginLaser.otherPortal;
        laser = pluginLaser.laser;
        sourceDir = pluginLaser.sourceDir;
        islandId = pluginLaser.islandId;
        originalIslandId = pluginLaser.originalIslandId;
        rockdata = pluginLaser.rockdata;
        t5RockBS = pluginLaser.t5RockBS;
        t5Sprites = pluginLaser.t5Sprites;
        laserUIOffMap = pluginLaser.laserUIOffMap;
        this.uILaserManager = uILaserManager;
        this.button = button;
    }

    public void Init()
    {
        SaveSprites();
        ResetSprites();
        if(centerObject == LaserCenterObject.SOURCE)
            uILaserManager.AddSource(this);
    }

    private void SaveSprites()
    {
        originalSprites = new Sprite[4];
        for(int i = 0 ; i < 4; i++)
        {
            originalSprites[i] = sprites[i].GetComponent<Image>().sprite;
        }
    }

    public void ResetSprites()
    {
        foreach (GameObject s in sprites)
        {
            if (s!= null)
                s.SetActive(false);
        }
        foreach (GameObject s in emptysprites)
        {
            if (s!= null)
                s.SetActive(false);
        }
        if(t5RockBS)
        {
            foreach (GameObject s in t5Sprites)
            {
                if (s!= null)
                    s.SetActive(false);
            }
        }
        if (laserUIOffMap != null)
            laserUIOffMap.HideLaser();
        crossings = 0;
    }

    public void UpdateEdgeToCenter(int direction)
    {
        if (button.TileIsActive || MirageIsActive())
        {
            if (t5RockBS && direction == 1)
                t5Sprites[0].SetActive(true);
            else
            {
                sprites[direction].SetActive(true);
            }
            if (edgeblockers[direction])
                return;
        }
        else
            emptysprites[direction].SetActive(true);

        crossings++;
        UpdateCenter(direction);
    }

    private bool MirageIsActive()
    {
        if (MirageSTileManager.GetInstance() == null) return false;
        if (button == null) return false;
        return (islandId != button.islandId);
    }

    public void UpdateCenter(int direction)
    {
        crossings++;
        int nextDir = (direction + 2) % 4;

        if (!button.TileIsActive && !MirageIsActive())
        {
            UpdateCenterToEdge(nextDir);
            return;
        }

        switch (centerObject)
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
                otherPortal.laserUIData.UpdateCenterToEdge(nextDir);
                return;
            default:
                break;
        }
        UpdateCenterToEdge(nextDir);
    }

    public void UpdateCenterToEdge(int direction)
    {
        crossings++;

        if (button != null && (button.TileIsActive || MirageIsActive()))
        {
            if (t5RockBS && direction == 1)
                t5Sprites[1].SetActive(true);

            if (edgeblockers[direction])
                return;
                
            else if (!t5RockBS || direction != 1)
            {
                sprites[direction].SetActive(true);
            }
        }
        else if (emptysprites != null && emptysprites[direction] != null)
            emptysprites[direction].SetActive(true);

        if (crossings > MAX_CROSSINGS)
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
        if (button == null) return;
        int nextX = button.x + GetTileOffsetVector(direction)[0];
        int nextY = button.y + GetTileOffsetVector(direction)[1];

        if (button.y == 2 && button.x == 3 && nextX == 2)
        {
            laserUIOffMap.ShowLaser();
        }
        
        if (nextX < 0 || nextY < 0 || nextX > 5 || nextY > 2) return;
        if (button.x > 2 != nextX > 2) return;
        
        int nextDir = (direction + 2) % 4;
        foreach (ArtifactTileButton a in uILaserManager.buttonToLaserUI.Keys)
        {
            if(a.x == nextX && a.y == nextY)
            {
                uILaserManager.buttonToLaserUI[a].UpdateEdgeToCenter(nextDir);
            }
        }
    }

    public void UpdateSprites()
    {
        if (laser.isEnabled)
        {
            UpdateCenterToEdge(sourceDir);
        }
    }

    public void CopyDataFromMirageSource(LaserUIData original)
    {
        islandId = original.islandId;
        centerObject = original.centerObject;
        sourceDir = original.sourceDir;
        edgeblockers = original.edgeblockers;
        if (centerObject == LaserCenterObject.SOURCE)
        {
            uILaserManager.AddSource(this);
        }
        UpdateImages(original);
        uILaserManager.UpdateSpritesFromSource();
    }

    private void UpdateImages(LaserUIData original)
    {
        if (original.originalSprites == null) 
            original.SaveSprites();
        for (int i = 0 ; i < 4; i++)
        {
            var newSprite = original.originalSprites[i];
            if (newSprite != null)
                sprites[i].GetComponent<Image>().sprite = newSprite;
        }
    }

    public void ClearDataOnMirageDisable()
    {
        centerObject = LaserCenterObject.NONE;
        islandId = originalIslandId;
        edgeblockers = new bool[4];
        uILaserManager.RemoveSource(this);
        ResetImages();
        uILaserManager.UpdateSpritesFromSource();
    }

    private void ResetImages()
    {
        for (int i = 0 ; i < 4; i++)
        {
            sprites[i].GetComponent<Image>().sprite = originalSprites[i];
        }
    }

}
