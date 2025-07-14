using System;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    public GameObject timer;
    public UIBigText text;
 
    void Update()
    {
        bool shouldShowTimer = !PauseManager.IsPaused 
            && !GameUI.instance.isMenuScene 
            && SettingsManager.Setting<bool>(Settings.ShowTimer).CurrentValue;
        
        if (shouldShowTimer != timer.activeSelf)
        {
            timer.SetActive(shouldShowTimer);
        }

        if (!shouldShowTimer) return;

        float time = 0f;
        if (SaveSystem.Current != null)
        {
            time = SaveSystem.Current.GetPlayTimeInSeconds();
        }
        Debug.Log($"UITimer Update called : {time}");
        TimeSpan ts = TimeSpan.FromSeconds(time);
        text.SetText(string.Format(
            "{0:D2}:{1:D2}:{2:D2}",
            ts.Hours,
            ts.Minutes,
            ts.Seconds
        ));
    }
}
