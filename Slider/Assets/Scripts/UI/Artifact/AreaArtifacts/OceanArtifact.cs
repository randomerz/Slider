using System.Collections.Generic;
using UnityEngine;

public class OceanArtifact : UIArtifact
{

    public override void SelectButton(ArtifactTileButton button) 
    {
        // do nothing
    }
    
    // temporary
    public void RotateTiles(int x, int y, bool rotateCCW)
    {
        List<Vector2Int> SMoveRotateArr = new List<Vector2Int> { 
                new Vector2Int(x, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x + 1, y + 1),
                new Vector2Int(x + 1, y),
            };

        List<ArtifactTileButton> tb = new List<ArtifactTileButton>{
            GetButton(x, y),
            GetButton(x, y + 1),
            GetButton(x + 1, y + 1),
            GetButton(x + 1, y)
        };

        if (rotateCCW) 
        {
            SMoveRotateArr.Reverse();
            tb.Reverse();
        }

        for (int i=3; i>=0; i--)
        {
            int curX = SMoveRotateArr[i].x;
            int curY = SMoveRotateArr[i].y;

            STile[,] grid = SGrid.current.GetGrid();

            if (grid[curX, curY].hasAnchor)
            {
                SMoveRotateArr.RemoveAt(i);
                tb.RemoveAt(i);
            }
        }

        SMove rotate = new SMoveRotate(SMoveRotateArr);
        // todo: if can rotate
        SGrid.current.Move(rotate);
        
        for (int i=0; i<tb.Count; i++)
        {
            tb[i].SetPosition(SMoveRotateArr[(i+1) % tb.Count].x, SMoveRotateArr[(i + 1) % tb.Count].y);
        }



    }
}