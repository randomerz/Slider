using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactLavafallPlugin : ArtifactTBPlugin
{
    public UISpriteSwapper spriteSwapper;

    private void OnEnable()
    {
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        bool shouldBeActive = true; 
        if(button.y != 3)
            shouldBeActive = false;
        if(UIArtifact.GetButton(button.x, 2) != null && UIArtifact.GetButton(button.x, 2).TileIsActive)
            shouldBeActive = false;
        if(shouldBeActive)
            spriteSwapper.TurnOn();
        else
            spriteSwapper.TurnOff();
    }
}
