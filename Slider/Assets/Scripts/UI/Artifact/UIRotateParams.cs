using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotateParams : MonoBehaviour
{
    public int bottomLeftX;
    public int bottomLeftY;
    public UIArtifact artifact;

    public void OnClick()
    {
        artifact.RotateTiles(bottomLeftX, bottomLeftY);
    }
}
