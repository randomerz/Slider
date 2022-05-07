using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainArtifactButton : ArtifactTileButton
{
    private Vector3 xOffset = new Vector3(28, -15);
    private Vector3 yOffset = new Vector3(28, 15);
    private Vector3 zOffset = new Vector3(0, 59);
    public Vector3 button1Position;


    public override void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 pos = button1Position + x * xOffset + (y % 2) * yOffset + (y / 2) * zOffset;
        GetComponent<RectTransform>().anchoredPosition = pos;
    }
}
