using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class SavePanelManager : MonoBehaviour, IDialogueTableProvider
{
    public enum SaveMode 
    {
        Normal,
        Delete,
        Backup,
    }

    public class SaveModeArgs : System.EventArgs
    {
        public SaveMode mode;
    }
    public static System.EventHandler<SaveModeArgs> OnSaveModeChanged;

    public Dictionary<string, Localization.LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<SaveMode, string>
        {
            {
                SaveMode.Normal,
                "Select Save"
            },
            {
                SaveMode.Delete,
                "Delete file?"
            },
            {
                SaveMode.Backup,
                "Restore a backup?"
            },
        }
    );

    [SerializeField] private NewSavePanelManager newSavePanelManager;
    [SerializeField] private UIMenu savePanel;
    [SerializeField] private UIMenu newSavePanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject confirmButtonGameObject;

    // We skip save picking if there are no saves and go straight to the new save menu. When we hit escape, we want
    // to come back here and not immediately go *back* to the new save menu.
    private bool hasAlreadySkippedSavePicking = false;

    private MainMenuSaveButton buttonToConfirm;
    private float timeUntilSwap;
    private const float BASE_TIME_UNTIL_SWAP = 0.5f;
    private bool isHidingDescriptions;

    public SaveMode CurrentMode { get; private set; } = SaveMode.Normal;

    public void OpenSaves()
    {
        if (!AreAnyProfilesLoaded() && !hasAlreadySkippedSavePicking)
        {
            hasAlreadySkippedSavePicking = true;
            OpenNewSave(0);
            return;
        }

        SetMode(SaveMode.Normal);
    }

    private void OnDisable()
    {
        hasAlreadySkippedSavePicking = false;
        ClearButtonToConfirm();
    }
    
    private void Update()
    {
        if (buttonToConfirm != null)
        {
            timeUntilSwap -= Time.deltaTime;
            if (timeUntilSwap <= 0)
            {
                isHidingDescriptions = !isHidingDescriptions;
                buttonToConfirm.SetForceHideDescriptions(isHidingDescriptions);
                timeUntilSwap = BASE_TIME_UNTIL_SWAP;
            }
        }
    }

    public void ToggleDeleteMode()
    {
        SetMode(CurrentMode == SaveMode.Delete ? SaveMode.Normal : SaveMode.Delete);
    }

    public void ToggleBackupMode()
    {
        SetMode(CurrentMode == SaveMode.Backup ? SaveMode.Normal : SaveMode.Backup);
    }

    public void SetMode(SaveMode mode)
    {
        ClearButtonToConfirm();

        titleText.text = this.GetLocalized(mode).TranslatedFallbackToOriginal;

        CurrentMode = mode;
        OnSaveModeChanged?.Invoke(this, new SaveModeArgs { mode = CurrentMode });
    }

    public void SetButtonToConfirm(MainMenuSaveButton button)
    {
        buttonToConfirm = button;
        timeUntilSwap = BASE_TIME_UNTIL_SWAP;
        confirmButtonGameObject.SetActive(button != null);
    }

    public void ClearButtonToConfirm()
    {
        if (buttonToConfirm != null)
        {
            buttonToConfirm.SetForceHideDescriptions(false);
        }
        buttonToConfirm = null;
        isHidingDescriptions = false;
        confirmButtonGameObject.SetActive(false);
    }

    public void OnClickConfirm()
    {
        if (buttonToConfirm == null)
        {
            Debug.LogError($"Clicked confirm but there was no button to confirm.");
            return;
        }

        buttonToConfirm.OnClickConfirm();
        ClearButtonToConfirm();
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
