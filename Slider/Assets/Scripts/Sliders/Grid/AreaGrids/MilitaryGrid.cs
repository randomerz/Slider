using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryGrid : SGrid
{
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
        
    }

    private void OnDisable()
    {
        
    }

    public override void Save()
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }


    // === Military puzzle specific ===

    public void RestartCombat()
    {
        Debug.Log("Restarting Combat!");
    }
}
