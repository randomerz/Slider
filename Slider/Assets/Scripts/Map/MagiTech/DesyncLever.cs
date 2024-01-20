using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncLever : Lever
{
    [SerializeField] private DesyncLever leverPair;
    [SerializeField] private GameObject lightning;

    private STile currentTile;
    private bool isPastLever;
    private bool isDesynced;

    //public bool IsDesynced { get => isDesynced; }

    private void Start()
    {
        currentTile = SGrid.GetSTileUnderneath(gameObject);
        isPastLever = MagiTechGrid.IsInPast(transform);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MagiTechGrid.OnDesyncStartWorld += OnDesyncStartWorld;
        MagiTechGrid.OnDesyncEndWorld += OnDesyncEndWorld;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        MagiTechGrid.OnDesyncStartWorld -= OnDesyncStartWorld;
        MagiTechGrid.OnDesyncEndWorld -= OnDesyncEndWorld;
    }

    public override void Switch()
    {
        base.Switch();
        if(isPastLever && !isDesynced && !leverPair.isDesynced)
        {
            leverPair.Switch();
        }
    }
    
    private void OnDesyncStartWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = currentTile.islandId == e.desyncIslandId || currentTile.islandId == e.anchoredTileIslandId;
        lightning.SetActive(isDesynced);
    }

    private void OnDesyncEndWorld(object sender, MagiTechGrid.OnDesyncArgs e)
    {
        isDesynced = false;
        lightning.SetActive(false);
        UpdateOtherState();
    }

    public void UpdateOtherState()
    {
        if(!isPastLever || isDesynced) return;
        if(leverPair._targetVisualOn != _targetVisualOn)
            leverPair.Switch();
    }
}
