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
            LocalizationLoader.ForceReload();
        }
        // for non-main menu scenes, just apply the setting and tell players to restart
        else
        {
            popup.SetActive(true); // there's no way to just swap out every different pixel font at once, just tell the player to cope
        }
    }
}
