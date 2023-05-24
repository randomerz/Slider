using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSGridAnimator : SGridAnimator
{
    protected override Coroutine DetermineMoveType(SMove move, STile[,] grid, Movement m)
    {
        int diff = m.endLoc.y - m.startLoc.y;
        bool onSameLevel = (Mathf.Abs(diff) <= 1); 

        STile tile = Player.GetInstance().GetSTileUnderneath();

        bool isPlayerOnStile = (tile != null && tile.islandId == grid[m.startLoc.x, m.startLoc.y].islandId);
        if(!onSameLevel && isPlayerOnStile)
        {
            if(diff > 0)
                CameraZoom.MoveUp(movementDuration);
            else
                CameraZoom.MoveDown(movementDuration);
        }
        return StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move, onSameLevel));
    }

    protected override void EffectOnMoveStart(SMove move, Transform root, STile tile)
    {
        base.EffectOnMoveStart(move, root, tile);
        if(move is SMoveLayerSwap && tile.isTileActive) {
            if(Player.GetInstance().GetSTileUnderneath() == tile) {
                //Player on tile, dither world
                ((MountainSTile) tile).AnimateBorderTileDither(movementDuration);
            }
            else {
                //Player not on tile, dither tile
                ((MountainSTile) tile).AnimateTileDither(movementDuration);
            }
        }
    }
}
