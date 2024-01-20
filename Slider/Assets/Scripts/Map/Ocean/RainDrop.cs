using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class RainDrop : MonoBehaviour
{
    public IObjectPool<RainDrop> pool;
    public Animator animator;

    public TileBase waterTile;

    public void ResetDrop()
    {
        // reset animator
        animator.SetTrigger("reset");
        animator.SetBool("onLand", false);
        animator.SetBool("onWater", false);
        animator.SetBool("onNothing", false);
    }

    public void Contact()
    {
        if (Vector3.Distance(transform.position, Player.GetPosition()) > 15)
        {
            // if its too far dont do anything crazy
            animator.SetBool("onNothing", true);
            return;
        }

        // set animator to correct splash: water, land, void
        STile stile = SGrid.GetSTileUnderneath(gameObject);
        Tilemap map = GetTilemap(stile);
        TileBase tileBase = map == null ? null : map.GetTile(map.WorldToCell(transform.position));

        if (tileBase == null)
        {
            animator.SetBool("onNothing", true);
            return;
        }
        else if (tileBase == waterTile)
        {
            animator.SetBool("onWater", true);
            if (stile != null) transform.SetParent(stile.transform);
            return;
        }
        else
        {
            animator.SetBool("onLand", true);
            if (stile != null) transform.SetParent(stile.transform);
            return;
        }
    }

    private Tilemap GetTilemap(STile stile)
    {
        if (stile == null)
        {
            STileTilemap fallback = SGrid.Current.GetWorldGridTilemaps();
            if (fallback == null) 
                return null;
            else 
                return fallback.materials;
        }
        else
        {
            if (stile.stileTilemaps == null)
                return null;
            else
                return stile.stileTilemaps.materials;
        }
    }

    public void FinishDrop()
    {
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
