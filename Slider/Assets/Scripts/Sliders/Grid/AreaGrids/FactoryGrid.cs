using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : SGrid
{

    public override void Init() {
        InitArea(Area.Factory);
        base.Init();
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
