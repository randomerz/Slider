using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveOnPowered : MonoBehaviour
{
    private bool isPowered;
    ArtifactTileButton button;

    private void Awake()
    {
        isPowered = false;
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        //Kinda wasteful, but not a big deal.
        if (button != null && button.isTileActive)
        {
            gameObject.SetActive(isPowered);
        }
    }

    public void OnHandlerSetActive(ElectricalNode.OnPoweredArgs e)
    {
        bool activeInScene = true;
        var button = GetComponentInParent<ArtifactTileButton>();
        if (button != null && !button.isTileActive)
        {
            activeInScene = false;
        }

        if (activeInScene)
        {
            gameObject.SetActive(e.powered);
        }
        isPowered = e.powered;
    }
}

//This is it? This is the script I've heard so much about and feared all this time? Yes it is.