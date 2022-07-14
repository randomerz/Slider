using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : TimeTravelGrid
{
    //[SerializeField] private STile[] altStiles;

    //[SerializeField] private STile[,] altGrid;

    public override void Init()
    {
        InitArea(Area.MagiTech);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("MagiTech");
        UIEffects.FadeFromBlack();
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
