using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPowerTracker : MonoBehaviour
{
    [SerializeField] private Image poweredImage;
    [SerializeField] private ElectricalNode myNode;

    private ArtifactTileButton button;

    private void Awake()
    {
        if (myNode == null)
        {
            Debug.LogError("UIPowerTracker requires node to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void OnEnable()
    {
        SetImageActiveStatus();
        myNode.OnPowered.AddListener(OnMyNodePowered);
    }

    private void OnDisable()
    {
        myNode.OnPowered.RemoveListener(OnMyNodePowered);
    }

    private void SetImageActiveStatus()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;
        poweredImage.enabled = myNode.Powered && !nodeOnDisabledButton;
    }

    public void OnMyNodePowered(ElectricalNode.OnPoweredArgs e)
    {
        SetImageActiveStatus();
    }
}

//This is it? This is the script I've heard so much about and feared all this time? Yes it is.