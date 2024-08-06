using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MountainSoftlockChecker : MonoBehaviour
{
    public Tilemap boxOfSadness;
    private Anchor anchor;
    public Meltable ice;

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEndLate += OnSTileMoveEnd;
        Anchor.OnAnchorInteract += OnAnchorInteract;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEndLate -= OnSTileMoveEnd;
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }
    
    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs e)
    {
        anchor = sender as Anchor;
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if(e.stile.islandId == 1 && e.stile.y <= 1)
        {
            if(CheckSoftlock())
            {
                HandleSoftlock();
            }
        }
    }

    private bool CheckSoftlock()
    {
        return SGrid.Current.GetNumTilesCollected() < 3 && ice.IsBrokenOrMelted() && (PlayerInBoxOfSadness() != AnchorInBoxOfSadness());
    }

    private bool PosInBoxOfSadness(Vector3 pos)
    {
        if (pos.x < -100) return false;
        TileBase tile = boxOfSadness.GetTile(boxOfSadness.WorldToCell(pos));
        return tile != null && boxOfSadness.ContainsTile(tile);
    }

    private bool PlayerInBoxOfSadness()
    {
        if(Player.GetInstance() == null) return false;
        return PosInBoxOfSadness(Player.GetPosition());
    }

    private bool AnchorInBoxOfSadness()
    {
        if(anchor == null) return false;
        return PosInBoxOfSadness(anchor.transform.position);
    }

    private void HandleSoftlock()
    {
        PlayerInventory.ReturnAnchorFromMap();
    }
}
