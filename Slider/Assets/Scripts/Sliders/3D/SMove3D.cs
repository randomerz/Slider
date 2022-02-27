using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: A representation of a move made on the artifact, as well as the borders around the move that prevent the player from clipping.
public class SMove3D
{
    public List<Movement> moves = new List<Movement>(); // move tile at (x, y) to (z, w)

    //L: Every (x, y) position that is touched by moves
    public HashSet<Vector3Int> positions = new HashSet<Vector3Int>();   
    //L: key - (x, y) position of a tile
    //L: Value - A list of values taken from 0, 1, 2, 3 (maximum of 4 values) for each tile side denoting which borderse are on.
    public Dictionary<Vector3Int, List<int>> borders = new Dictionary<Vector3Int, List<int>>();

    public Dictionary<Vector3Int, List<int>> GenerateBorders()
    {
        positions.Clear();
        borders.Clear();

        foreach (Movement m in moves)
        {
            // for the cases where its swapping more than two
            for (int x = Mathf.Min(m.startLoc.x, m.endLoc.x); x <= Mathf.Max(m.startLoc.x, m.endLoc.x); x++)
                for (int y = Mathf.Min(m.startLoc.y, m.endLoc.y); y <= Mathf.Max(m.startLoc.y, m.endLoc.y); y++)
                    for (int z = Mathf.Min(m.startLoc.z, m.endLoc.z); z <= Mathf.Max(m.startLoc.z, m.endLoc.z); z++)
                        positions.Add(new Vector3Int(x, y, z));
        }

        foreach (Vector3Int p in positions)
        {
            AddBorder(p, 0, p + Vector3Int.right);
            AddBorder(p, 1, p + Vector3Int.up);
            AddBorder(p, 2, p + Vector3Int.left);
            AddBorder(p, 3, p + Vector3Int.down);
        }

        return borders;
    }

    //L: side - 0, 1, 2, 3 corresponding to right, up, left, and down sides of a tile
    private void AddBorder(Vector3Int pos1, int side, Vector3Int pos2)
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
    public virtual bool Overlaps(SMove3D other)
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

        foreach (Vector3Int pos in positions)
        {
            if (other.positions.Contains(pos))
            {
                return true;
            }
        }

        return false;
    }
}

//a movment between 2 points, stored as a pair of vector 3s (this is a logical way to store this type of information)
public class Movement 
{
    public Vector3Int startLoc;
    public Vector3Int endLoc;

    public Movement(Vector3Int s, Vector3Int e)
    {
        startLoc = s;
        endLoc = e;
    }

    public Movement(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        startLoc = new Vector3Int(x1, y1, z1);
        endLoc = new Vector3Int(x2, y2, z2);
    }
}



//L: Swapping includes moving a tile to an empty spot!
public class SMoveSwap3D : SMove3D
{
    public SMoveSwap3D(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        moves.Add(new Movement(x1, y1, z1, x2, y2, z2));
        moves.Add(new Movement(x2, y2, z2, x1, y1, z1));
    }

    public Movement GetSwapAsMovement()
    {
        return moves[0];
    }
}

public class SSlideSwap3D : SMove3D
{
    public SSlideSwap3D(List<Movement> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            moves.Add(new Movement(points[i].startLoc, points[i].endLoc));
        }
    }

    public override bool Overlaps(SMove3D other)
    {
        return true;
    }
}