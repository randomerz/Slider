using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSTile : STile
{
    public override void Init()
    {
        STILE_WIDTH = 25;
        base.Init();
    }




    public override Vector3 calculatePosition(int x, int y) 
    {
        return new Vector3(x * STILE_WIDTH, y/2 * ((MountainGrid) SGrid.current).layerOffset + y % 2 * STILE_WIDTH);
    }

    public override void SetMovingPosition(Vector2 position)
    {
        Vector3 newPos = STILE_WIDTH * new Vector3(position.x, position.y);

        if(position.y >= 2)
            newPos += new Vector3(0, ((MountainGrid)SGrid.current).layerOffset - 2 * STILE_WIDTH);

        // physics
        Vector3 dr = newPos - transform.position;
        UpdateTilePhysics(dr);


        transform.position = newPos;
        SetTileMapPositions(newPos);
    }
}
