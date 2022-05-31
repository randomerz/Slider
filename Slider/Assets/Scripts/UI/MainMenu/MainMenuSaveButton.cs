using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuSaveButton : MonoBehaviour
{
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI completionText;
    public TextMeshProUGUI timeText;
    public Image catSticker;

    [SerializeField] private int profileIndex = -1;
    private SaveProfile profile;

    public static bool deleteMode;

    public MainMenuManager mainMenuManager;

    private void OnEnable() 
    {
        ReadProfileFromSave();
        UpdateButton();
    }

    public void UpdateButton()
    {

        if (profile != null)
        {
            completionText.gameObject.SetActive(true);
            timeText.gameObject.SetActive(true);

            // name based on delete mode
            if (!deleteMode)
                profileNameText.text = profile.GetProfileName();
            else
                profileNameText.text = "Delete?";
            completionText.text = string.Format("{0}/9", profile.GetAreaToSGridData().Keys.Count);
            float seconds = profile.GetPlayTimeInSeconds();
            int minutes = (int)seconds / 60;
            timeText.text = string.Format("{0}h{1:D2}", minutes / 60, minutes % 60);
            catSticker.enabled = profile.GetCompletionStatus();
        }
        else
        {
            profileNameText.text = "[ Empty ]";
            completionText.gameObject.SetActive(false);
            timeText.gameObject.SetActive(false);
            catSticker.enabled = false;
        }
    }
    
    public void ReadProfileFromSave()
    {
        SerializableSaveProfile ssp = SaveSystem.GetSerializableSaveProfile(profileIndex);
        if (ssp != null)
            profile = ssp.ToSaveProfile();
        else
            profile = null;
        SaveSystem.SetProfile(profileIndex, profile);
    }

    public void OnClick()
    {
        if (profile == null)
        {
            // create new profile
            mainMenuManager.OpenNewSave(profileIndex);
        }
        else
        {
            if (deleteMode)
            {
                Debug.Log("Deleteing profile " + profileIndex);
                DeleteThisProfile();
            }
            else
            {
                // load my profile
                LoadThisProfile();
            }
        }
    }

    private void LoadThisProfile()
    {
        SaveSystem.LoadSaveProfile(profileIndex);
    }

    public void DeleteThisProfile()
    {
        if (profile != null)
        {
            // TODO: seek confirmation
            SaveSystem.DeleteSaveProfile(profileIndex);
            profile = null;
            SaveSystem.SetProfile(profileIndex, profile);
            UpdateButton();
        }
    }
}
