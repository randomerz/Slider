using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechGrid : TimeTravelGrid
{
    public static MagiTechGrid instance;

    //[SerializeField] private STile[] altStiles;

    //[SerializeField] private STile[,] altGrid;

    public override void Init()
    {
        myArea = Area.MagiTech;

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
