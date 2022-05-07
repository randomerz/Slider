using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainArtifactButton : ArtifactTileButton
{
    public int z;

    private Vector3 xOffset = new Vector3(28, -15);
    private Vector3 yOffset = new Vector3(28, 15);
    private Vector3 zOffset = new Vector3(0, 59);
    public Vector3 button1Position;


    public void SetPosition(int x, int y, int z)
    {
        //Debug.Log("Current position: " + this.x + "," + this.y);
        this.x = x;
        this.y = y;
        this.z = z;
        //Debug.Log("New position: " + this.x + "," + this.y);

        Vector3 pos = button1Position + x * xOffset + y * yOffset + z * zOffset;
        GetComponent<RectTransform>().anchoredPosition = pos;
    }
}
