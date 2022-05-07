using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSTile : STile
{
    public override void Init()
    {
        STILE_WIDTH = 25;
        // SetTileActive(isTileActive);
        // DC: this is so that we can call any other relevant functions when STiles are enabled in SGrid
        if (isTileActive) 
        {
            SGrid.current.EnableStile(this, false);
        }
        else
        {
            SetTileActive(isTileActive); 
        }

        Vector3 defaultPos = calculatePosition(x, y);
        transform.position = defaultPos;
        SetTileMapPositions(defaultPos);
        //sliderColliderDisableCount = 0;
    }

    /*TODO:
    -Border collider stuff
    */



    public Vector3 calculatePosition(int x, int y) 
    {
        return new Vector3(x, y/2 * MountainGrid.instance.layerOffset + y % 2 * STILE_WIDTH);
    }

    public override void SetGridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = calculatePosition(x, y);

        //StartCoroutine(StartCameraShakeEffect());

        if (isTileActive)
        {
            // animations and style => physics on tile
            Vector3 dr = newPos - transform.position;
            UpdateTilePhysics(dr);

            transform.position = newPos;
            SetTileMapPositions(newPos);
        }
        else
        {
            transform.position = newPos;
            SetTileMapPositions(newPos);
        }
    }

    public override void SetGridPositionRaw(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 newPos = calculatePosition(x, y);
        transform.position = newPos;
        SetTileMapPositions(newPos);
    }

    public override void SetMovingPosition(Vector2 position)
    {
        Vector3 newPos = calculatePosition((int) position.x, (int) position.y);

        // physics
        Vector3 dr = newPos - transform.position;
        UpdateTilePhysics(dr);


        transform.position = newPos;
        SetTileMapPositions(newPos);
    }
}
