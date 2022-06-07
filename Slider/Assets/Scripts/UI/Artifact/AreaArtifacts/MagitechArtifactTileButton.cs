using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechArtifactTileButton : ArtifactTileButton
{
    public override void SetPosition(int x, int y)
    {
        base.SetPosition((x % 3), y);
    }
}
