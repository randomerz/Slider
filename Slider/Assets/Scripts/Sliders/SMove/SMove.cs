using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: A representation of a move made on the artifact, as well as the borders around the move that prevent the player from clipping.
public class SMove
{
    public List<Movement> moves = new List<Movement>(); // move tile at (x1, y1) to (x2, y2)

    //L: Every (x, y) position that is touched by moves
    public HashSet<Vector2Int> positions = new HashSet<Vector2Int>();   
    //L: key - (x, y) position of a tile
    //L: Value - A list of values taken from 0, 1, 2, 3 (maximum of 4 values) for each tile side denoting which borderse are on.
    public Dictionary<Vector2Int, List<int>> borders = new Dictionary<Vector2Int, List<int>>();

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

        foreach (Vector2Int p in positions)
        {
            AddBorder(p, 0, p + Vector2Int.right);
            AddBorder(p, 1, p + Vector2Int.up);
            AddBorder(p, 2, p + Vector2Int.left);
            AddBorder(p, 3, p + Vector2Int.down);
        }

        return borders;
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
}

//C: a movment between 2 points, stored as a pair of vector 2s
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

    public Movement GetSwapAsVector()
    {
        return moves[0];
    }
}

public class SMoveLayerSwap: SMove
{
    public SMoveLayerSwap(int x1, int y1, int x2, int y2, int islandId1, int islandId2)
    {
        moves.Add(new Movement(x1, y1, x2, y2, islandId1));
        moves.Add(new Movement(x2, y2, x1, y1, islandId2));
    }

    public Movement GetSwapAsVector()
    {
        return moves[0];
    }

    public override Dictionary<Vector2Int, List<int>> GenerateBorders()
    {
        positions.Clear();
        borders.Clear();

        foreach (Movement m in moves)
        {
            positions.Add(new Vector2Int(m.startLoc.x, m.startLoc.y));
            positions.Add(new Vector2Int(m.endLoc.x, m.endLoc.y));
        }

        foreach (Vector2Int p in positions)
        {
            AddBorder(p, 0, p + Vector2Int.right);
            AddBorder(p, 1, p + Vector2Int.up);
            AddBorder(p, 2, p + Vector2Int.left);
            AddBorder(p, 3, p + Vector2Int.down);
        }

        /*
         * C:
         * if we are starting from y=0 or y=2, then the sum of start and end y is 2. We should not have a top
         * border on y=1. If we are starting from y=1 or y-3, then the sum of the start and end y is 4.
         * We should not have a bottom border on y=2
         *
         */
        if(moves[0].startLoc.y + moves[0].endLoc.y == 2)
            borders[new Vector2Int(moves[0].startLoc.x, 1)].Remove(1);
        else
            borders[new Vector2Int(moves[0].startLoc.x, 2)].Remove(3);        
        return borders;
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