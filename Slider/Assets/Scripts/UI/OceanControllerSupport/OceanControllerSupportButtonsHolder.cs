using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanControllerSupportButtonsHolder : MonoBehaviour
{
    [SerializeField] private OceanControllerSupportButton[] oceanControllerSupportButtons;

    public OceanControllerSupportButton lastControllerSupportButtonClicked;

    public void ColorAllButtonsBasedOnIfSelected()
    {
        foreach (OceanControllerSupportButton button in oceanControllerSupportButtons)
        {
            button.ColorButtonBasedOnIfSelected();
        }
    }

    public void MakeLastControllerButtonClickedDisappear()
    {
        if (lastControllerSupportButtonClicked == null)
        {
            Debug.LogError($"[Controller] last controller support button was null! This may or may not be an error");
            return;
        }
        StartCoroutine(lastControllerSupportButtonClicked.DisappearThenReappearAfterTime(0.8f));
    }
}
