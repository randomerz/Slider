using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleGrid : SGrid
{
    public static JungleGrid instance;

    private new void Awake() {
        myArea = Area.Jungle;

        foreach (Collectible c in collectibles) 
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;
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
        
        AudioManager.PlayMusic("Connection");
        UIEffects.FadeFromBlack();
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
