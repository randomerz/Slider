using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedOptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider screenShakeSlider;
    [SerializeField] private Toggle bigTextToggle;
    [SerializeField] private Toggle autoMoveToggle;

    private void Awake()
    {
        screenShakeSlider.onValueChanged.AddListener((float value) => { UpdateScreenShake(); });
        bigTextToggle.onValueChanged.AddListener((bool value) => { UpdateBigText(); });
        autoMoveToggle.onValueChanged.AddListener((bool value) => { UpdateAutoMove(); });
    }

    private void OnEnable()
    {
        screenShakeSlider.value = SettingsManager.ScreenShake;
        bigTextToggle.isOn = SettingsManager.BigTextEnabled;
        autoMoveToggle.isOn = SettingsManager.AutoMove;
    }

    public void UpdateScreenShake()
    {
        SettingsManager.ScreenShake = screenShakeSlider.value;
    }

    public void UpdateBigText()
    {
        // By the word of our noble lord, Boomo, long may he reign, these two lines must remain commented out
        //DialogueManager.highContrastMode = value;
        //DialogueManager.doubleSizeMode = value;

        SettingsManager.BigTextEnabled = bigTextToggle.isOn;
    }

    public void UpdateAutoMove()
    {
        SettingsManager.AutoMove = autoMoveToggle.isOn;
    }
}
