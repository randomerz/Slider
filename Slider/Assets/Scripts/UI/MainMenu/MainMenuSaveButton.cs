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

    public MainMenuManager mainMenuManager;

    private void OnEnable() 
    {
        UpdateButton();
    }

    private void UpdateButton()
    {
        profile = SaveSystem.GetProfile(profileIndex);

        if (profile != null)
        {
            completionText.gameObject.SetActive(true);
            timeText.gameObject.SetActive(true);

            profileNameText.text = profile.GetProfileName();
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

    public void OnClick()
    {
        if (profile == null)
        {
            // create new profile
            mainMenuManager.OpenNewSave(profileIndex);
        }
        else
        {
            // load my profile
            LoadThisProfile();
        }
    }

    private void LoadThisProfile()
    {
        SaveSystem.SetCurrentProfile(profileIndex);
        mainMenuManager.StartGameWithCurrentSave();
    }
}
