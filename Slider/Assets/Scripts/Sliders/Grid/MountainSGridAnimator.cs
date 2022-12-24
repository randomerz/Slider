using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSGridAnimator : SGridAnimator
{
    protected override Coroutine DetermineMoveType(SMove move, STile[,] grid, Movement m)
    {
        int diff = m.endLoc.y - m.startLoc.y;
        bool onSameLevel = (Mathf.Abs(diff) <= 1); 

        bool isPlayerOnStile = true;//(Player.GetInstance().GetSTileUnderneath() != null &&
                                //Player.GetInstance().GetSTileUnderneath().islandId == grid[m.startLoc.x, m.startLoc.y].islandId);
        if(!onSameLevel && isPlayerOnStile)
        {
            if(diff > 0)
                CameraZoom.MoveUp(movementDuration);
            else
                CameraZoom.MoveDown(movementDuration);
        }
        return StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move, onSameLevel));
    }
}
