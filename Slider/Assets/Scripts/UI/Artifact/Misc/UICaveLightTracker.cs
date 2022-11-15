using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICaveLightTracker : MonoBehaviour
{
    [SerializeField] private Image poweredImage;
    [SerializeField] private CaveLight myLight;

    private ArtifactTileButton button;
    private bool playerInPast;

    private void Awake()
    {
        if (myLight == null)
        {
            Debug.LogError("UIPowerTracker requires node to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void OnEnable()
    {
        UpdatePoweredImageEnabled();
    }

    private void OnDisable()
    {
        
    }

    private void UpdatePoweredImageEnabled()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;
        poweredImage.enabled = myLight.LightOn && !nodeOnDisabledButton && !playerInPast;
    }
}