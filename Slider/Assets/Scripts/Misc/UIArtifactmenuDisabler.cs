using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Animation event moment :(
public class UIArtifactmenuDisabler : MonoBehaviour
{
    public UIArtifactMenus uiArtifactMenus;

    [SerializeField] private OceanControllerSupportButtonsHolder oceanControllerSupportButtonsHolder;
    [SerializeField] private Button topLeftControllerButton;

    public void DisableArtPanel()
    {
        uiArtifactMenus.DisableArtPanel();
    }


    private void OnEnable()
    {
        //Debug.LogWarning("SELECT!!");

        if (Player.GetInstance().GetCurrentControlScheme() == "Controller")
        {
            oceanControllerSupportButtonsHolder.gameObject.SetActive(true);
            topLeftControllerButton.Select();
            //Debug.LogError("Control SELECT!!");
            oceanControllerSupportButtonsHolder.ColorAllButtonsBasedOnIfSelected();
        }
        else { oceanControllerSupportButtonsHolder.gameObject.SetActive(false); }
    }
}
