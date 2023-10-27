using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePanelManager : MonoBehaviour
{
    [SerializeField] private NewSavePanelManager newSavePanelManager;

    private bool skippedSavePicking;

    public void OpenSaves()
    {
        if (!AreAnyProfilesLoaded() && !skippedSavePicking)
        {
            skippedSavePicking = true;
            newSavePanelManager.OpenNewSave(0);
            return;
        }

        MainMenuSaveButton.SetDeleteMode(false);
    }

    public void ToggleDeleteMode()
    {
        MainMenuSaveButton.ToggleDeleteMode();
    }

    private bool AreAnyProfilesLoaded()
    {
        return SaveSystem.GetProfile(0) != null || SaveSystem.GetProfile(1) != null || SaveSystem.GetProfile(2) != null;
    }
}
