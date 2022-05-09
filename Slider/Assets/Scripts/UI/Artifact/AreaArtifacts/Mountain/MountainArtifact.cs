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
            if(b != null && !b.isTileActive)
                moveOptionButtons.Add(b);
        }

        return moveOptionButtons;
    }
}
