using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wire : ConductiveElectricalNode
{
    private Tilemap tm;
    private GameObject sparks;
    private List<GameObject> sparkInstances;



    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        tm = GetComponent<Tilemap>();
        sparks = Resources.Load<GameObject>("WireSparks");
        sparkInstances = new List<GameObject>();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => WireDatabase.Instance != null);
        SetTiles();
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        SetTiles();

        if (e.powered)
        {
            //Do Sparks
            foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)
            {

                if (tm.GetTile(pos) != null)
                {
                    if (WireDatabase.Instance.Sparks.ContainsKey(tm.GetSprite(pos)))
                    {
                        GameObject sparkInstance = Instantiate(sparks, tm.CellToWorld(pos) + tm.tileAnchor, Quaternion.identity, transform);
                        sparkInstance.GetComponent<WireSparks>().StartSparks(WireDatabase.Instance.Sparks[tm.GetSprite(pos)]);

                        SpriteRenderer sr = sparkInstance.GetComponent<SpriteRenderer>();
                        TilemapRenderer tr = GetComponent<TilemapRenderer>();
                        sr.sortingLayerID = tr.sortingLayerID;
                        sr.sortingOrder = tr.sortingOrder;

                        sparkInstances.Add(sparkInstance);
                    } else
                    {
                        Debug.LogError($"Sparks not found for tile at position {pos} with sprite {tm.GetSprite(pos)}");
                    }

                }
            }

            foreach (var n in neighbors)
            {
                if (conductionPoints != null && conductionPoints.ContainsKey(n))
                {
                    ChangeToAlt((Vector3Int) TileUtil.WorldToTileCoords(conductionPoints[n]), true);
                }
            }
        } else
        {
            foreach (GameObject go in sparkInstances)
            {
                Destroy(go);
            }

            foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)
            {
                if (tm.GetTile(pos) != null)
                {
                    ChangeToAlt(pos, false);
                }
            }

            sparkInstances.Clear();
        }
  
    }

    private void SetTiles()
    {
        WireDatabase.Instance.SwapTiles(tm, Powered);
    }

    //L: This is the sketchiest code I've written in a while.
    public void ChangeToAlt(Vector3Int pos, bool useAlt)
    {
        TileBase alt = null;

        //Check directions near the position because it might be off by 1 (Yes IK this is stupid)
        Vector3Int[] dirs = { Vector3Int.zero, Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down, 
                                Vector3Int.down + Vector3Int.left, Vector3Int.left + Vector3Int.up, Vector3Int.up + Vector3Int.right, Vector3Int.right + Vector3Int.down};
        
        foreach (Vector3Int dir in dirs)
        {
            Vector3Int currPos = pos + dir;
            TileBase tile = tm.GetTile(currPos);
            if (useAlt && tile != null && WireDatabase.Instance.ConductingAltOn.ContainsKey(tile))
            {
                alt = WireDatabase.Instance.ConductingAltOn[tile];
            }


            if (!useAlt && tile != null && WireDatabase.Instance.ConductingAltOff.ContainsKey(tile))
            {
                alt = WireDatabase.Instance.ConductingAltOff[tile];
            }

            if (alt != null)
            {
                tm.SetTile(currPos, alt);
                break;
            }
        }
        
    }
}
