using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PixelFontToggleLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject popup;
    
    private void OnEnable()
    {
        popup.SetActive(false);
    }

    public void OnToggle(bool toggleIsOn)
    {
        if (!toggleIsOn)
        {
            SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).SetCurrentValue(true);
        }
        
        // for main menu scene, apply setting directly
        if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
        {
            if (LocalizationLoader.CurrentLocale == Localization.LocalizationFile.DefaultLocale)
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
            else
            {
                // No need to reload scene for non-English locales since the pixel font is uniform
                LocalizationLoader.RefreshLocalization();
            }
        }
        // for non-main menu scenes, just apply the setting and tell players to restart
        else
        {
            // Ideally, since non-English locales all use the same pixel font, just refreshing the localization
            // without going back to main menu would work (i.e. not crash the game and get fonts changed correctly)
            // However...
            // TODO: ensure no overriding translation (refresh causing stale text to override dynamically set .text values from non-localization code,
            //       especially with dialogue table providers...)
            // if (LocalizationLoader.CurrentLocale == Localization.LocalizationFile.DefaultLocale)
            // {
            //     popup.SetActive(
            //         true); // there's no way to just swap out every different pixel font at once, just tell the player to cope
            // }
            // else
            // {
            //     LocalizationLoader.RefreshLocalization();
            // }
            
            popup.SetActive(
                true); // there's no way to just swap out every different pixel font at once, just tell the player to cope
        }
    }
}
