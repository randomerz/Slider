using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPowerTracker : MonoBehaviour
{
    [SerializeField]
    private bool isPowered;
    [SerializeField]
    private Image poweredImage;

    private ArtifactTileButton button;


    private void Awake()
    {
        isPowered = false;
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void OnEnable()
    {
        SetActiveStatus();
    }

    private void Update()
    {
        //Kinda wasteful, but not a big deal.
        SetActiveStatus();
    }

    private void SetActiveStatus()
    {
        bool onDisabledButton = button != null && !button.isTileActive;
        if (onDisabledButton)
        {
            poweredImage.enabled = false;
        }
        else
        {
            poweredImage.enabled = isPowered;
        }
    }

    public void OnHandlerSetActive(ElectricalNode.OnPoweredArgs e)
    {
        isPowered = e.powered;
    }
}

//This is it? This is the script I've heard so much about and feared all this time? Yes it is.