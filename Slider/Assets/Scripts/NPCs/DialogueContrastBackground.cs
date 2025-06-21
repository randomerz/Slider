using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueContrastBackground : MonoBehaviour
{
    public float borderWidth = 0.5f;
    public Vector3 offset = new Vector3(0, 0.6f);

    public TextMeshProUGUI text;
    public RectTransform backgroundRectTransform;

    public bool toggleImageBasedOnSettings = false; // if you want to this script to handle itself,
                                                    // NPCs handle it in DialogueDisplay.cs
    public Image imageToToggle;

    private void Update()
    {
        if (toggleImageBasedOnSettings)
        {
            CheckContrast();
        }

        Bounds bounds = text.bounds;
        backgroundRectTransform.anchoredPosition = bounds.center + offset;
        backgroundRectTransform.sizeDelta = 2 * (bounds.extents + new Vector3(borderWidth, borderWidth));
    }

    private void CheckContrast()
    {
        if (imageToToggle == null)
        {
            return;
        }

        bool highContrastMode = SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).CurrentValue;
        bool emptyMessage = text.text.Trim().Length == 0;
        imageToToggle.enabled = highContrastMode && !emptyMessage;
    }
}
