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
        StartCoroutine(lastControllerSupportButtonClicked.DisappearThenReappearAfterTime(0.8f));
    }
}
