using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainArtifact : UIArtifact
{

    // replaces adjacentButtons
    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        moveOptionButtons.Clear();
        Vector3Int[] dirs;
        if(button.y % 2 == 0 ) {
            dirs = new Vector3Int[5] {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.up,
            Vector3Int.up * 2,
            Vector3Int.down * 2
            };
        } else {
            dirs = new Vector3Int[5] {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.down,
            Vector3Int.down * 2,
            Vector3Int.up * 2
            };
        }
        
        foreach (Vector3Int dir in dirs)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            if(b != null && !b.TileIsActive)
                moveOptionButtons.Add(b);
        }

        return moveOptionButtons;
    }

    protected override SMove ConstructMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        SMove move;
        //If swapping layers, the difference in y values will be 2
        if(Mathf.Abs(buttonCurrent.y - buttonEmpty.y) < 2) {
            move = new SMoveMountainSwap(buttonCurrent.x, buttonCurrent.y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId, isLayerSwap: false);
        }
        else {
            move = new SMoveMountainSwap(buttonCurrent.x, buttonCurrent.y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId, isLayerSwap: true);
        }
        return move;
    }

    public void AnchorSwap(STile s1, STile s2)
    {
        //We can't just call CheckandSwap because CanMove will return false due to the anchor
        SMove move = new SMoveMountainSwap(s1.x, s1.y, s2.x, s2.y, s1.islandId, s2.islandId, isLayerSwap: true);
        QueueMoveFromButtonPair(move, GetButton(s1.islandId), GetButton(s2.islandId));
    }
}
