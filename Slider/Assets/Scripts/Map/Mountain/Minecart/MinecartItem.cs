using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MinecartItem : Item
{
    public Minecart mc;

    public override STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        Collider2D hit = Physics2D.OverlapPoint(dropLocation, LayerMask.GetMask("Slider"));
        if (hit == null || hit.GetComponent<STile>() == null)
        {
            gameObject.transform.parent = null;
            //Debug.LogWarning("Player isn't on top of a slider!");
            return null;
        }
        STile hitTile = hit.GetComponent<STile>();
        Tilemap railmap = hitTile.stileTileMaps.GetComponent<STileTilemap>().minecartRails;
        RailManager rm = railmap.GetComponent<RailManager>();
        mc.railManager = rm;
        rm.mc = mc;
        StartCoroutine(AnimateDrop(railmap.CellToWorld(railmap.WorldToCell(dropLocation)) + mc.offSet, callback));
        mc.SnapToTile(railmap.WorldToCell(dropLocation));
        gameObject.transform.parent = hitTile.transform.Find("Objects").transform;
        return hitTile;
    }

    public override void OnEquip()
    {
        mc.StopMoving();
        mc.resetTiles();
        base.OnEquip();
    }
}
