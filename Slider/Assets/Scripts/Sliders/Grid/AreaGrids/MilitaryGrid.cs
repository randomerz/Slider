using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryGrid : SGrid
{
    public MilitarySpriteTable militarySpriteTable; // global reference

    public override void Init()
    {
        InitArea(Area.Military);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Military");
    }

    private void OnEnable()
    {
        OnGridMove += OnTileMove;
    }

    private void OnDisable()
    {
        OnGridMove -= OnTileMove;
    }

    public override void Save()
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }


    // === Military puzzle specific ==

    public void OnTileMove(object sender, OnGridMoveArgs e)
    {
        MilitaryTurnManager.EndPlayerTurn();
    }
}
