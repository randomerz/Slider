using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PixelFontToggleLogic : MonoBehaviour
{
    public void OnToggle(bool toggleIsOn)
    {
        if (!toggleIsOn)
        {
            SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).SetCurrentValue(true);
        }
        LocalizationLoader.RefreshLocalization();
    }
}
