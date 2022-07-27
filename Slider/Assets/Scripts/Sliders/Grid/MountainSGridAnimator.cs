using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSGridAnimator : SGridAnimator
{
    protected override void DetermineMoveType(SMove move, STile[,] grid, Movement m)
    {
        bool onSameLevel = (Mathf.Abs(m.endLoc.y - m.startLoc.y) <= 1); 
        StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move, onSameLevel));
    }
}
