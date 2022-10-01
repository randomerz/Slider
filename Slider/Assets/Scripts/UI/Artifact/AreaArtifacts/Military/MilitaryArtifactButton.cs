using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MilitaryArtifactButton : ArtifactTileButton
{
    protected override void Start()
    {
        base.Start();
        // this.GetComponentInChildren<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }
    protected override void SetAnchoredPos(int x, int y)
    {
        // X, Y positions are in [-42, -14, 14, 42]
        Vector2 pos = new Vector2(-42 + x * 28, -42 + y * 28);
        GetComponent<RectTransform>().anchoredPosition = pos;
    }
}
