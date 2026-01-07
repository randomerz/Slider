using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Localization;

public class MainMenuSaveButton : MonoBehaviour, IDialogueTableProvider
{
    [SerializeField] private SavePanelManager savePanelManager;
    [SerializeField] private Image buttonBackgroundImage;
    [SerializeField] private Sprite lightGrayBackgroundSprite;
    [SerializeField] private Sprite darkGrayBackgroundSprite;

    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI completionText;
    public TextMeshProUGUI timeText;
    public Image catSticker;
    public Image breadge;

    [SerializeField] private int profileIndex = -1;
    private SaveProfile profile;
    private SaveProfile profileBackup;

    private bool forceHidingDescriptions;
    private bool isLoading;
    
    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<string, string>
        {
            { "ProfileEmpty", "[ Empty ]" },
            { "ProfileLoading", "Loading..." },
        });

    private const string RAINBOW_BREADGE_ACQUIRED = "MagiTechRainbowBreadgeAcquired";
    private const string DID_CHEAT = "UsedCheats";
    private const string DID_TELEPORT = "UsedTeleport";


    private void OnEnable() 
    {
        ReadProfileFromSave();
        SavePanelManager.OnSaveModeChanged += UpdateButton;
    }

    private void OnDisable()
    {
        SavePanelManager.OnSaveModeChanged -= UpdateButton;
    }

    public void UpdateButton(object sender, SavePanelManager.SaveModeArgs e)
    {
        profileNameText.text = GetDisplayName(e.mode);
        SetTextColorGray(e.mode != SavePanelManager.SaveMode.Normal);
        isLoading = e.mode == SavePanelManager.SaveMode.Loading;

        switch (savePanelManager.CurrentMode)
        {
            case SavePanelManager.SaveMode.Normal:
            case SavePanelManager.SaveMode.Delete:
            case SavePanelManager.SaveMode.Loading: // can be any
                SetButtonDescriptions(profile);
                break;

            case SavePanelManager.SaveMode.Backup:
                SetButtonDescriptions(profileBackup);
                break;

            default:
                Debug.LogError($"Save mode was not recognized.");
                break;
        }
    }

    public void SetForceHideDescriptions(bool value)
    {
        forceHidingDescriptions = value;
        UpdateButton(this, new SavePanelManager.SaveModeArgs { mode = savePanelManager.CurrentMode });
    }

    private void SetButtonDescriptions(SaveProfile prof)
    {
        bool isNullOrForceHide = prof == null || forceHidingDescriptions || isLoading;

        profileNameText.gameObject.SetActive(!forceHidingDescriptions);

        completionText.gameObject.SetActive(!isNullOrForceHide);
        timeText.gameObject.SetActive(!isNullOrForceHide);
        catSticker.gameObject.SetActive(!isNullOrForceHide);
        breadge.gameObject.SetActive(!isNullOrForceHide);

        if (isNullOrForceHide)
        {
            return;
        }

        float seconds = prof.GetPlayTimeInSeconds();
        int minutes = (int)seconds / 60;
        bool didCheatOrTeleport = prof.GetBool(DID_CHEAT) || prof.GetBool(DID_TELEPORT);

        completionText.text = string.Format("{0}/9", GetNumAreasCompleted(prof));
        timeText.text = string.Format("{0}h{1:D2}", minutes / 60, minutes % 60);

        catSticker.gameObject.SetActive(prof.GetCompletionStatus());
        breadge.gameObject.SetActive(prof.GetBool(RAINBOW_BREADGE_ACQUIRED));

        buttonBackgroundImage.sprite = didCheatOrTeleport ? darkGrayBackgroundSprite : lightGrayBackgroundSprite;
    }

    private string GetDisplayName(SavePanelManager.SaveMode mode)
    {
        return mode switch
        {
            SavePanelManager.SaveMode.Normal => profile != null ? profile.GetProfileName() : this.GetLocalizedSingle("ProfileEmpty"),
            SavePanelManager.SaveMode.Delete => profile != null ? $"{profile.GetProfileName()}?" : this.GetLocalizedSingle("ProfileEmpty"),
            SavePanelManager.SaveMode.Backup => profileBackup != null ? $"{profileBackup.GetProfileName()}?" : this.GetLocalizedSingle("ProfileEmpty"),
            SavePanelManager.SaveMode.Loading => this.GetLocalizedSingle("ProfileLoading"),
            _ => this.GetLocalizedSingle("ProfileEmpty"),
        };
    }

    private void SetTextColorGray(bool shouldBeGray)
    {
        Color c = shouldBeGray ? GameSettings.darkGray : GameSettings.black;
        profileNameText.color = c;
        completionText.color = c;
        timeText.color = c;
    }

    private int GetNumAreasCompleted(SaveProfile profile)
    {
        int count = 0;
        foreach (Area area in Area.GetValues(typeof(Area)))
        {
            if (area == Area.None) continue;

            SGridData data = profile.GetSGridData(area);
            if (data == null) 
            {
                Debug.LogError("SGridData was null when trying to read number of complete areas!");
                continue;
            }

            if (data.completionColor == ArtifactWorldMapArea.AreaStatus.color)
            {
                count += 1;
            }
        }
        return count;
    }

    public void ReadProfileFromSave()
    {
        if (!SaveSystem.AreSavesReady())
        {
            Debug.Log("[Saves] Saves are not ready yet, cannot read profile.");
            return;
        }

        SerializableSaveProfile ssp = SaveSystem.GetSerializableSaveProfile(profileIndex);
        if (ssp != null)
            profile = ssp.ToSaveProfile();
        else
            profile = null;

        SerializableSaveProfile sspBackup = SaveSystem.GetBackupSerializableSaveProfile(profileIndex);
        if (sspBackup != null)
            profileBackup = sspBackup.ToSaveProfile();
        else
            profileBackup = null;

        SaveSystem.SetProfile(profileIndex, profile);
    }

    public void OnClick()
    {
        switch (savePanelManager.CurrentMode)
        {
            case SavePanelManager.SaveMode.Normal:
                if (profile == null)
                {
                    savePanelManager.OpenNewSave(profileIndex);
                    return;
                }

                LoadThisProfile();
                break;

            case SavePanelManager.SaveMode.Backup:
                savePanelManager.SetButtonToConfirm(this);
                break;

            case SavePanelManager.SaveMode.Delete:
                if (profile == null)
                {
                    savePanelManager.OpenNewSave(profileIndex);
                    return;
                }

                savePanelManager.SetButtonToConfirm(this);
                break;

            case SavePanelManager.SaveMode.Loading:
                Debug.Log("[Saves] Saves are still loading, doing nothing...");
                break;

            default:
                Debug.LogError($"Save mode was not recognized.");
                break;
        }
    }

    public void OnClickConfirm()
    {
        switch (savePanelManager.CurrentMode)
        {
            case SavePanelManager.SaveMode.Normal:
                break;

            case SavePanelManager.SaveMode.Backup:

                RestoreBackupProfile();
                savePanelManager.ClearButtonToConfirm();
                break;

            case SavePanelManager.SaveMode.Delete:
                if (profile == null)
                {
                    savePanelManager.OpenNewSave(profileIndex);
                    return;
                }

                DeleteThisProfile();
                savePanelManager.ClearButtonToConfirm();
                break;

            case SavePanelManager.SaveMode.Loading:
                Debug.Log("[Saves] Saves are still loading, doing nothing...");
                break;

            default:
                Debug.LogError($"Save mode was not recognized.");
                break;
        }
    }

    private void LoadThisProfile()
    {
        UIEffects.FadeToBlack();
        SaveSystem.LoadSaveProfile(profileIndex);
    }

    public void DeleteThisProfile()
    {
        if (profile != null)
        {
            // TODO: seek confirmation
            SaveSystem.DeleteSaveProfile(profileIndex);
            profile = null;
            UpdateButton(this, new SavePanelManager.SaveModeArgs { mode = savePanelManager.CurrentMode });
        }
    }

    public void RestoreBackupProfile()
    {
        if (profileBackup != null)
        {
            SaveSystem.RestoreBackupProfile(profileIndex);
            ReadProfileFromSave();
            savePanelManager.SetMode(SavePanelManager.SaveMode.Normal);
        }
    }

}
