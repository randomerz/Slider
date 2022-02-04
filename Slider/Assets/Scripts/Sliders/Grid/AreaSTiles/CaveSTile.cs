using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveSTile : STile
{
    private bool isShadowed;

    public void Start()
    {
        isShadowed = false;

        SGridAnimator.OnSTileMove += UpdateIsShadowed;
    }

    public void SetShadowed(bool value)
    {
        isShadowed = value;
        
        //Update tile visuals to reflect being shadowed.
    }

    private void UpdateIsShadowed(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile == this)
        {
            SetShadowed((SGrid.current as CaveGrid).GetLit(this.x, this.y));
        }
    }
}
