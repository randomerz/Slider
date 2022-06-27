using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : TimeTravelGrid
{
    public static FactoryGrid instance;

    public override void Init() {
        myArea = Area.Factory;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Init();

        instance = this;
        SetSingleton();
    }


    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Factory");
        UIEffects.FadeFromBlack();

        //SGrid.OnGridMove += (sender, e) => { Debug.Log(GetGridString()); };
    }

    public override void Save() 
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }
}
