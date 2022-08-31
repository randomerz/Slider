using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPowerTracker : MonoBehaviour
{
    [SerializeField] private Image poweredImage;
    [SerializeField] private ElectricalNode myNode;

    private ArtifactTileButton button;
    private bool playerInPast;

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
        UpdatePoweredImageEnabled();
        myNode.OnPowered.AddListener(OnMyNodePowered);
        FactoryGrid.playerPastChanged += PlayerPastChangedHandler;
    }

    private void OnDisable()
    {
        myNode.OnPowered.RemoveListener(OnMyNodePowered);
        FactoryGrid.playerPastChanged -= PlayerPastChangedHandler;
    }

    private void PlayerPastChangedHandler(bool inPast)
    {
        playerInPast = inPast;
        UpdatePoweredImageEnabled();
    }

    private void UpdatePoweredImageEnabled()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;
        poweredImage.enabled = myNode.Powered && !nodeOnDisabledButton && !playerInPast;
    }

    public void OnMyNodePowered(ElectricalNode.OnPoweredArgs e)
    {
        UpdatePoweredImageEnabled();
    }
}

//This is it? This is the script I've heard so much about and feared all this time? Yes it is.