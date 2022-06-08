using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechSTile : STile
{
    public override Vector3 calculatePosition(int x, int y) 
    {
        return new Vector3(x/3 * ((MagiTechGrid) SGrid.current).gridOffset + x % 3 * STILE_WIDTH, y * STILE_WIDTH);
    }

    public override Vector3 calculateMovingPosition(float x, float y) 
    {
        Vector3 newPos = STILE_WIDTH * new Vector3(x, y);

        if(x >= 3)
            newPos += new Vector3(((MagiTechGrid)SGrid.current).gridOffset - 3 * STILE_WIDTH, 0);
        return newPos;
    }
}
