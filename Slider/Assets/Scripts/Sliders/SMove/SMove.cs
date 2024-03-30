using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: A representation of a move made on the artifact, as well as the borders around the move that prevent the player from clipping.
public class SMove
{
    public float duration = 1.0f;   // Normalized to movement duration in SGridAnimator
    public bool forceFullDuration = false; // must be manually set
    public List<Movement> moves = new(); // Should be set when SMove is created
    public HashSet<Vector2Int> positions = new(); // A set of positions that the SMove covers (usually the endpoints of various Movements)
    public Dictionary<Vector2Int, List<int>> borders = new(); // (pos of tile, borders {0, 1, 2, 3})

    public int moveCounter; // for internal use by SGridAnimator usually

    protected static Vector2Int NONE_VECTOR = new Vector2Int(-999999, -999999);

    public virtual Dictionary<Vector2Int, List<int>> GenerateBorders()
    {
        positions.Clear();
        borders.Clear();

        foreach (Movement m in moves)
        {
            // for the cases where its swapping more than two
            for (int x = Mathf.Min(m.startLoc.x, m.endLoc.x); x <= Mathf.Max(m.startLoc.x, m.endLoc.x); x++)
            {
                for (int y = Mathf.Min(m.startLoc.y, m.endLoc.y); y <= Mathf.Max(m.startLoc.y, m.endLoc.y); y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        AddBordersByPositions(positions);

        return borders;
    }

    protected virtual void AddBordersByPositions(HashSet<Vector2Int> positions)
    {
        foreach (Vector2Int p in positions)
        {
            AddBorder(p, 0, p + Vector2Int.right);
            AddBorder(p, 1, p + Vector2Int.up);
            AddBorder(p, 2, p + Vector2Int.left);
            AddBorder(p, 3, p + Vector2Int.down);
        }
    }

    //L: side - 0, 1, 2, 3 corresponding to right, up, left, and down sides of a tile
    protected void AddBorder(Vector2Int pos1, int side, Vector2Int pos2)
    {
        if (!borders.ContainsKey(pos1))
            borders.Add(pos1, new List<int>());
        if (!borders.ContainsKey(pos2))
             borders.Add(pos2, new List<int>());

        // toggle sides
        /*
         * L: We want to toggle the sides as opposed to just adding them because if a border is hit twice, it means that it is in the path of the move and is actually not a border.
         * Ex: If you are moving from (0, 1) to (1,1). 
         * First process (0,1) after which The top, left, bottom, and right sides of (0,1) will be added to the list. The left side of (1,1) is added when (1,1) is pos2.
         * Next, while (1,1) is being processed, since the left side has already been added, it will be removed, while the top, right, and bottom borders are added.
         *  ___ ___                              _______                       
         * |   |   | - without toggling         |       | - with toggling
         * |___|___|                            |_______|
         *
         */
        if (borders[pos1].Contains(side))
            borders[pos1].Remove(side);
        else
            borders[pos1].Add(side);
            //L: The sides are with respect to pos1, so the opposite side is added for pos2 (think geometrically with 2 tiles that share a side)
        if (borders[pos2].Contains((side + 2) % 4))
            borders[pos2].Remove((side + 2) % 4);
        else
            borders[pos2].Add((side + 2) % 4);
        
    }

    // DC: check if this SMove and other SMove share the same (x, y) in position
    public virtual bool Overlaps(SMove other)
    {
        if (other == null) return false;

        if (positions.Count == 0)
        {
            GenerateBorders();
        }
        if (other.positions.Count == 0)
        {
            other.GenerateBorders();
        }

        foreach (Vector2Int pos in positions)
        {
            if (other.positions.Contains(pos))
            {
                return true;
            }
        }

        return false;
    }

    public Vector3 GetMoveTilesCenter()
    {
        Vector3 center = Vector3.zero;
        int i = 0;
        foreach(Movement m in moves)
        {
            if(m.islandId > 0 && SGrid.Current.GetStile(m.islandId).isTileActive)
            {
                center += SGrid.Current.GetStile(m.islandId).transform.position;
                i++;
            }
        }
        if(i > 0)
            center /= i;
        return center;
    }

    public int GetFirstActiveTile()
    {
        foreach (Movement m in moves)
            if (SGrid.Current.GetStile(m.islandId).isTileActive)
                return m.islandId;
        Debug.LogWarning("No active tiles found in move. This should not happen");
        return -1;
    }
}

//C: a movement between 2 points, stored as a pair of vector 2s
public class Movement 
{
    public Vector2Int startLoc;
    public Vector2Int endLoc;
    public int islandId;

    public Movement(Vector2Int s, Vector2Int e, int islandId)
    {
        startLoc = s;
        endLoc = e;
        this.islandId = islandId;
    }

    public Movement(int x1, int y1, int x2, int y2, int islandId)
    {
        startLoc = new Vector2Int(x1, y1);
        endLoc = new Vector2Int(x2, y2);
        this.islandId = islandId;
    }
}

//L: Swapping includes moving a tile to an empty spot!
public class SMoveSwap : SMove
{
    public SMoveSwap(int x1, int y1, int x2, int y2, int islandId1, int islandId2)
    {
        moves.Add(new Movement(x1, y1, x2, y2, islandId1));
        moves.Add(new Movement(x2, y2, x1, y1, islandId2));
    }
}

public class SMoveMountainSwap: SMove
{
    public bool isLayerSwap = false;

    public SMoveMountainSwap(int x1, int y1, int x2, int y2, int islandId1, int islandId2, bool isLayerSwap=false)
    {
        moves.Add(new Movement(x1, y1, x2, y2, islandId1));
        moves.Add(new Movement(x2, y2, x1, y1, islandId2));
        this.isLayerSwap = isLayerSwap;
    }

    // In Mountain, the world is a 2x4 grid but the world is two separate 2x2 grids.
    // Moves on the top of mountain (y=1) shouldn't trigger borders on bottom of mountain (y=2)
    protected override void AddBordersByPositions(HashSet<Vector2Int> positions)
    {
        foreach (Vector2Int p in positions)
        {
            if (p.y == 1) // Bottom of mountain
                AddBorder(p, 1, NONE_VECTOR);
            else
                AddBorder(p, 1, p + Vector2Int.up);

            if (p.y == 2) // Top of mountain
                AddBorder(p, 3, NONE_VECTOR);
            else
                AddBorder(p, 3, p + Vector2Int.down);

            AddBorder(p, 0, p + Vector2Int.right);
            AddBorder(p, 2, p + Vector2Int.left);
        }
    }
}

//C: used in magitech to move a tile in the past/present at the same time
public class SMoveMagiTechMove : SMove
{
    public bool shouldSync;
    public int altIslandId1;
    public int altIslandId2;

    public SMoveMagiTechMove(int x1, int y1, int x2, int y2, int islandId1, int islandId2, int altIslandId1, int altIslandId2, bool shouldSync=true)
    {
        this.shouldSync = shouldSync;
        this.altIslandId1 = altIslandId1;
        this.altIslandId2 = altIslandId2;

        moves.AddRange(new SMoveSwap(x1, y1, x2, y2, islandId1, islandId2).moves);

        if (shouldSync)
        {
            if (altIslandId1 == -1 || altIslandId2 == -1)
            {
                Debug.LogError("Tried making a sync move but alt island ids were not provided!");
                return;
            }
            Vector2Int alt1 = MagiTechArtifact.FindAltCoords(x1, y1);
            Vector2Int alt2 = MagiTechArtifact.FindAltCoords(x2, y2);
            moves.AddRange(new SMoveSwap(alt1.x, alt1.y, alt2.x, alt2.y, altIslandId1, altIslandId2).moves);
        }
    }

    // In MagiTech, the world is a 6x3 grid but the world is two separate 3x3 grids.
    // Moves in the right side of present (x=2) shouldn't trigger borders on left side of past (x=3) and vice versa
    protected override void AddBordersByPositions(HashSet<Vector2Int> positions)
    {
        foreach (Vector2Int p in positions)
        {
            if (p.x == 2) // Right side of present
                AddBorder(p, 0, NONE_VECTOR);
            else
                AddBorder(p, 0, p + Vector2Int.right);

            if (p.x == 3) // Left side of past
                AddBorder(p, 2, NONE_VECTOR);
            else
                AddBorder(p, 2, p + Vector2Int.left);

            AddBorder(p, 1, p + Vector2Int.up);
            AddBorder(p, 3, p + Vector2Int.down);
        }
    }

    //C: basically just modulus. Used to find corresponding values on either side of the grid
    private int FindAlt(int num, int offset)
    {
        return (num + offset) % (offset * 2);
    }

    public Movement GetSwapAsVector()
    {
        return moves[0];
    }

    public Transform[] GetTileTransforms()
    {
        Transform[] transforms = new Transform[2];
        foreach(Movement m in moves)
        {
            STile s = SGrid.Current.GetStile(m.islandId);
            if(s!=null && s.isTileActive)
            {   
                transforms[s.islandId > 9 ? 1 : 0] = s.transform;
            }
        }
        return transforms;
    }
}

public class SMoveLinkedSwap : SMove
{
    public SMoveLinkedSwap(int x1, int y1, int x2, int y2, 
                                int linkx, int linky, int islandId, int linkIslandId)
    {
        moves.Add(new Movement(x1, y1, x2, y2, islandId));

        int dx = x2 - x1;
        int dy = y2 - y1;
        moves.Add(new Movement(linkx, linky, linkx + dx, linky + dy, linkIslandId));

        //L: Need to handle the edge case where the link tile moves to the prev tile's position
        // DC: IMPORTANT: we are currently not handling the islandId for linked swaps for the inactive tiles
        if (linkx+dx == x1 && linky+dy == y1)
        {
            //L: Move the empty spot to where the link tile used to be (which is now empty)
            moves.Add(new Movement(x2, y2, linkx, linky, -1));
        } else
        {
            //L: Move both empty spots to where the cooresponding tile used to be (like with normal swaps)
            moves.Add(new Movement(linkx + dx, linky + dy, linkx, linky, -1));
            moves.Add(new Movement(x2, y2, x1, y1, -1));
        }
    }
}

//L: Used primarily in the "Ocean" area for rotating tiles around
public class SMoveRotate : SMove
{
    public bool isCCW;

    public List<Vector2Int> anchoredPositions; // DC: this is for bugs that having a tile anchored might cause

    public SMoveRotate(List<Vector2Int> points, List<int> islandIds, bool isCCW)
    {
        for (int i = 0; i < points.Count; i++)
        {
            moves.Add(new Movement(points[i], points[(i + 1) % points.Count], islandIds[i]));
        }

        this.isCCW = isCCW; 
    }
}

public class SMoveConveyor : SMove
{
    //Scuffy Rubby me Dubby Tubby
    public SMoveConveyor(List<Movement> moves)
    {
        this.moves = new List<Movement>(moves);
        this.duration = 0.5f;
    }
}

public class SSlideSwap : SMove
{
    public SSlideSwap(List<Movement> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            // Debug.Log(points[i].x + " " + points[i].y + " " + points[i].z + " " + points[i].w);
            moves.Add(new Movement(points[i].startLoc, points[i].endLoc, points[i].islandId));
        }
    }

    public override bool Overlaps(SMove other)
    {
        return true;
    }
}