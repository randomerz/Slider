using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SavePanelManager : MonoBehaviour
{
    [SerializeField] private NewSavePanelManager newSavePanelManager;
    [SerializeField] private UIMenu savePanel;
    [SerializeField] private UIMenu newSavePanel;

    // We skip save picking if there are no saves and go straight to the new save menu. When we hit escape, we want
    // to come back here and not immediately go *back* to the new save menu.
    private bool hasAlreadySkippedSavePicking = false;

    public void OpenSaves()
    {
        if (!AreAnyProfilesLoaded() && !hasAlreadySkippedSavePicking)
        {
            hasAlreadySkippedSavePicking = true;
            OpenNewSave(0);
            return;
        }

        MainMenuSaveButton.SetDeleteMode(false);
    }

    private void OnDisable()
    {
        hasAlreadySkippedSavePicking = false;
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
