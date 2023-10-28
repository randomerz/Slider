using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SavePanelManager : MonoBehaviour
{
    [SerializeField] private NewSavePanelManager newSavePanelManager;
    [SerializeField] private UIMenu savePanel;
    [SerializeField] private UIMenu newSavePanel;

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

    public void OpenNewSave(int profileIndex)
    {
        savePanel.MoveToMenu(newSavePanel);
        newSavePanelManager.OpenNewSave(profileIndex);
    }

    private bool AreAnyProfilesLoaded()
    {
        return SaveSystem.GetProfile(0) != null || SaveSystem.GetProfile(1) != null || SaveSystem.GetProfile(2) != null;
    }
}
