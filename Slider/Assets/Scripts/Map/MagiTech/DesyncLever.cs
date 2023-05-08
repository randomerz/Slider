using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesyncLever : Lever
{
    //Had to make pair DesyncLever because I apparently can't access the power state of a Lever in DesyncLever
    [SerializeField] private DesyncLever leverPair;
    [SerializeField] private GameObject lightning;
    private STile originTile;
    private bool isInPast;
    private bool isDesynced;

    public bool IsDesynced { get => isDesynced; }
    private void Start()
    {
        originTile = SGrid.GetSTileUnderneath(gameObject);
    }

    private new void OnEnable()
    {
        base.OnEnable();
        isInPast = MagiTechGrid.IsInPast(transform);
        Anchor.OnAnchorInteract += CheckLeverOnAnchorInteract;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        Anchor.OnAnchorInteract -= CheckLeverOnAnchorInteract;
    }
    
    public void UpdatePairState()
    {
        Debug.Log("desync: " + isDesynced + " power: " + _isPowered);
        if (!isDesynced) leverPair.SetState(!this._isPowered); //C: Because the actual power state doesn't update until after a coroutine 
    }

    private void CheckLeverOnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        isDesynced = e.drop && (leverPair.IsDesynced || (originTile != null && originTile.hasAnchor));
        if (!isDesynced) ResetToInitialAfterDesync();
        lightning.SetActive(isDesynced);
    }

    private void ResetToInitialAfterDesync()
    {
        //Whatever state leaves the first door open
        if (!isInPast) SetState(leverPair._isPowered);
    }
}
