using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SavePanelManager : MonoBehaviour, IDialogueTableProvider
{
    public enum SaveMode 
    {
        Normal,
        Delete,
        Backup,
        BackupFilesWarning,
        ConfirmDefaultText,
        ConfirmOpenFileVersion,
        Loading,
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
            {
                SaveMode.BackupFilesWarning,
                "You have <num/>\nbackup saves!"
            },
            {
                SaveMode.ConfirmDefaultText,
                "Confirm"
            },
            {
                SaveMode.ConfirmOpenFileVersion,
                "Open Folder"
            },
            {
                SaveMode.Loading,
                "Loading Saves..."
            },
        }
    );

    private const int NUMBER_OF_BACKUPS_TO_SHOW_WARNING = 100;

    [SerializeField] private NewSavePanelManager newSavePanelManager;
    [SerializeField] private UIMenu savePanel;
    [SerializeField] private UIMenu newSavePanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject confirmButtonGameObject;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI backupsWarningText;
    [SerializeField] private RectTransform confirmDefaultPos;
    [SerializeField] private RectTransform confirmBackUpPosition;
    [SerializeField] private MainMenuSaveButton[] saveButtons;

    // We skip save picking if there are no saves and go straight to the new save menu. When we hit escape, we want
    // to come back here and not immediately go *back* to the new save menu.
    private bool hasAlreadySkippedSavePicking = false;

    private MainMenuSaveButton buttonToConfirm;
    private float timeUntilSwap;
    private const float BASE_TIME_UNTIL_SWAP = 0.5f;
    private bool isHidingDescriptions;
    private bool shouldShowBackUpWarning;

    public SaveMode CurrentMode { get; private set; } = SaveMode.Normal;

    public void OpenSaves()
    {
        if (!AreAnyProfilesLoaded() && !hasAlreadySkippedSavePicking && SaveSystem.AreSavesReady())
        {
            hasAlreadySkippedSavePicking = true;
            OpenNewSave(0);
            return;
        }

        SetMode(SaveMode.Normal);
    }

    void OnEnable()
    {
        SaveSystem.OnGameSaveLoaded += OnSavesReady;
    }

    private void OnDisable()
    {
        hasAlreadySkippedSavePicking = false;
        ClearButtonToConfirm();

        SaveSystem.OnGameSaveLoaded -= OnSavesReady;
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

        if (!SaveSystem.AreSavesReady())
        {
            mode = SaveMode.Loading;
        }

        CurrentMode = mode;
        OnSaveModeChanged?.Invoke(this, new SaveModeArgs { mode = CurrentMode });
        CheckBackUpsWarning();
    }

    public void OnSavesReady(object sender, System.EventArgs e)
    {
        foreach (MainMenuSaveButton button in saveButtons)
        {
            button.ReadProfileFromSave();
        }

        if (CurrentMode == SaveMode.Loading)
        {
            SetMode(SaveMode.Normal);
        }
    }

    private void CheckBackUpsWarning()
    {
        if (!SaveSystem.AreSavesReady())
        {
            Debug.Log("[Saves] Saves are not ready yet, cannot read profile.");
            return;
        }

        int numBackUpFiles = SaveSystem.GetNumberOfPermanentBackups();
        shouldShowBackUpWarning = numBackUpFiles >= NUMBER_OF_BACKUPS_TO_SHOW_WARNING && CurrentMode == SaveMode.Normal;

        if (shouldShowBackUpWarning)
        {
            string text = IDialogueTableProvider.Interpolate(
                this.GetLocalizedSingle(SaveMode.BackupFilesWarning),
                new() {{ "num", numBackUpFiles.ToString() }}
            );
            backupsWarningText.text = text;
        }

        backupsWarningText.gameObject.SetActive(shouldShowBackUpWarning);
        confirmButtonText.text = shouldShowBackUpWarning ? 
            this.GetLocalizedSingle(SaveMode.ConfirmOpenFileVersion) :
            this.GetLocalizedSingle(SaveMode.ConfirmDefaultText);
        confirmButtonGameObject.SetActive(shouldShowBackUpWarning);
        confirmButtonGameObject.GetComponent<RectTransform>().anchoredPosition = shouldShowBackUpWarning ?
            confirmBackUpPosition.anchoredPosition :
            confirmDefaultPos.anchoredPosition;
    }

    public void SetButtonToConfirm(MainMenuSaveButton button)
    {
        if (buttonToConfirm != null)
        {
            buttonToConfirm.SetForceHideDescriptions(false);
        }
        buttonToConfirm = button;
        timeUntilSwap = BASE_TIME_UNTIL_SWAP;
        confirmButtonGameObject.SetActive(button != null || shouldShowBackUpWarning);
    }

    public void ClearButtonToConfirm()
    {
        if (buttonToConfirm != null)
        {
            buttonToConfirm.SetForceHideDescriptions(false);
        }
        buttonToConfirm = null;
        isHidingDescriptions = false;
        confirmButtonGameObject.SetActive(false || shouldShowBackUpWarning);
    }

    public void OnClickConfirm()
    {
        if (shouldShowBackUpWarning && CurrentMode == SaveMode.Normal)
        {
            Application.OpenURL(Application.persistentDataPath);
            return;
        }
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
