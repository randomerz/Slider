using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanControllerSupportButtonsHolder : MonoBehaviour
{
    [SerializeField] private OceanControllerSupportButton[] oceanControllerSupportButtons;

    public void ColorAllButtonsBasedOnIfSelected()
    {
        foreach (OceanControllerSupportButton button in oceanControllerSupportButtons)
        {
            button.ColorButtonBasedOnIfSelected();
        }
    }
}
