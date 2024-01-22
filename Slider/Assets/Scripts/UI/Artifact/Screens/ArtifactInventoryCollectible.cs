using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactInventoryCollectible : MonoBehaviour
{
    [Tooltip("Name that shows up in the inventory screen")]
    public string displayName;
    [Tooltip("ID of the collectible to be saved")]
    public string collectibleName;
    public Selectable controllerSelectible;
    public Image controllerSelectionImage;

    public UIArtifactInventory inventory;

    [HideInInspector] public bool isVisible;

    public void SetVisible(bool value)
    {
        isVisible = value;
        gameObject.SetActive(value);
    }

    public void UpdateInventoryName()
    {
        inventory.UpdateText(displayName);
    }

    private void OnEnable()
    {
        Player.OnControlSchemeChanged += ToggleNavigation;
        ToggleNavigation(Controls.CurrentControlScheme);
    }

    private void OnDisable()
    {
        Player.OnControlSchemeChanged -= ToggleNavigation;
    }

    private void ToggleNavigation(string s)
    {
        if (s == Controls.CONTROL_SCHEME_CONTROLLER)
        {
            EnableNavigation();
        } 
        else
        {
            DisableNavigation();
        }
    }

    private void DisableNavigation()
    {
        controllerSelectible.enabled = false;
        controllerSelectionImage.enabled = false;
    }

    private void EnableNavigation()
    {
        controllerSelectible.enabled = true;
    }
}
