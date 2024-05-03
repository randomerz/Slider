using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetSettingsToDefaults : MonoBehaviour
{
    public void ResetSettings()
    {
        SettingsManager.ResetAllSettingsToDefaults();
    }
}
