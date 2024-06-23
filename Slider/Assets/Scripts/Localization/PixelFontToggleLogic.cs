using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PixelFontToggleLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject popup;
    private Toggle toggle;

    private bool CurrentLocaleSupportsPixelFont =>
        LocalizationFile.SupportsPixelFont(SettingsManager.Setting<string>(Settings.Locale).CurrentValue);
    
    private void OnEnable()
    {
        toggle = GetComponent<Toggle>();
        popup.SetActive(false);
    }

    private void Update()
    {
        // AT: sorry,,,
        toggle.enabled = CurrentLocaleSupportsPixelFont;
    }

    public void OnToggle(bool toggleIsOn)
    {
        if (!toggleIsOn)
        {
            SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).SetCurrentValue(true);
        }

        // For locales not using pixel font anyway, don't refresh
        if (!CurrentLocaleSupportsPixelFont)
        {
            return;
        }
        
        // Otherwise, either directly apply the style or notify the player that it will be applied later
        if (toggleIsOn)
        {
            // TODO: get rid of this literal
            if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                popup.SetActive(
                    true); // there's no way to just swap out every different pixel font at once, just tell the player to cope
            }
        }
        else
        {
            popup.SetActive(false);
            // when switching from pixel font to a uniform fallback font, a simple refresh is ok without reload
            LocalizationLoader.RefreshLocalization();
        }
    }
}
