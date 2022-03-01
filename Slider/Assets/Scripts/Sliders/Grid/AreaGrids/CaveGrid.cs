using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGrid : SGrid
{
    public static CaveGrid instance;

    private bool[,] lightMap;

    public class OnLightMapUpdateArgs
    {
        public bool[,] lightMap;
    }
    public static event System.EventHandler<OnLightMapUpdateArgs> OnLightMapUpdate;

    private new void Awake() {
        myArea = Area.Caves;

        foreach (Collectible c in collectibles) 
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;

        lightMap = new bool[3, 3] { { false, false, true},
                                    { false, false, false}, 
                                    { false, false, true},
                                  };
    }
    

    void Start()
    {
        foreach (Collectible c in collectibles) 
        {
            if (PlayerInventory.Contains(c)) 
            {
                c.gameObject.SetActive(false);
            }
        }

        GetCollectible("Slider 5").gameObject.SetActive(false); // gameboy puzzle
        GetCollectible("Slider 6").gameObject.SetActive(true); // rat puzzle
        GetCollectible("Slider 7").gameObject.SetActive(false); // flashlight puzzle
        GetCollectible("Slider 9").gameObject.SetActive(false); // final puzzle
        
        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();
    }

    public bool GetLit(int x, int y)
    {
        return lightMap[x, y];
    }

    public void SetLit(int x, int y, bool value)
    {
        lightMap[x, y] = value;

        OnLightMapUpdate?.Invoke(this, new OnLightMapUpdateArgs { lightMap = this.lightMap });
    }

    public override void SaveGrid() 
    {
        base.SaveGrid();
    }

    public override void LoadGrid()
    {
        base.LoadGrid();
    }
}
