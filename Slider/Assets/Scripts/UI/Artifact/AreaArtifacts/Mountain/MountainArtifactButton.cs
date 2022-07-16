using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MountainArtifactButton : ArtifactTileButton
{
    private Vector3 xOffset = new Vector3(28, -14);
    private Vector3 yOffset = new Vector3(28, 14);
    private Vector3 zOffset = new Vector3(0, 59);
    public Vector3 button1Position;

    protected override void Start()
    {
        base.Start();
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }
    protected override void SetAnchoredPos(int x, int y)
    {
        Vector3 pos = button1Position + x * xOffset + (y % 2) * yOffset + (y / 2) * zOffset;
        GetComponent<RectTransform>().anchoredPosition = pos;
    }
}
