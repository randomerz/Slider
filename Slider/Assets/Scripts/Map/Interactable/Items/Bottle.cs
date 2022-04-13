using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : Item
{
    public bottleState state = bottleState.empty;
    public Sprite[] sprites;

    //Update sprite based on state?
    void Update()
    {
        if (state == bottleState.empty)
        {
            this.GetComponentInChildren<SpriteRenderer>().sprite = sprites[0];
        }
        else if (state == bottleState.cactus)
        {
            this.GetComponentInChildren<SpriteRenderer>().sprite = sprites[1];
        }
        else if (state == bottleState.dirty)
        {
            this.GetComponentInChildren<SpriteRenderer>().sprite = sprites[2];
        }
        else if (state == bottleState.clean)
        {
            this.GetComponentInChildren<SpriteRenderer>().sprite = sprites[3];
        }
    }

    public void SetState(int newState)
    {
        if (newState == 0)
        {
            state = bottleState.empty;
        }
        else if (newState == 1)
        {
            state = bottleState.cactus;
        }
        else if (newState == 2)
        {
            state = bottleState.dirty;
        }
        else if (newState == 3)
        {
            state = bottleState.clean;
        }
    }

    public void PurificationCheck()
    {
        if (this.state == bottleState.dirty)
        {
            this.state = bottleState.clean;
        }
    }

    public void ScoopDirtyWater()
    {
        if (this.state != bottleState.clean)
        {
            this.state = bottleState.dirty;
        }
    }
}
