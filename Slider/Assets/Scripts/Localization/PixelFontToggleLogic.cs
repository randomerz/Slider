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
        
        // for main menu scene, apply setting directly
        if (!SceneManager.GetActiveScene().name.Equals("MainMenu"))
        {
            if (toggleIsOn)
            {
                // to reload pixel font into the scene, a reload is required
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                // to unload pixel font (and apply fallback), just refresh
                LocalizationLoader.RefreshLocalization();
            }
        }
        // for non-main menu scenes, just apply the setting and tell players to restart
        else
        {
            popup.SetActive(
                true); // there's no way to just swap out every different pixel font at once, just tell the player to cope
        }
    }
}
