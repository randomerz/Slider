using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OceanControllerSupportEnabler : MonoBehaviour
{
    [SerializeField] private OceanControllerSupportButtonsHolder oceanControllerSupportButtonsHolder;
    [SerializeField] private Button topLeftControllerButton;

    private void OnEnable()
    {
        //wonkin' my willy rn
        if (oceanControllerSupportButtonsHolder != null)
        {
            if (Controls.UsingControllerOrKeyboardOnly())
            {
                oceanControllerSupportButtonsHolder.gameObject.SetActive(true);
                //topLeftControllerButton.Select();

                oceanControllerSupportButtonsHolder.ColorAllButtonsBasedOnIfSelected();
            }
            else { oceanControllerSupportButtonsHolder.gameObject.SetActive(false); }
        }
    }
}
