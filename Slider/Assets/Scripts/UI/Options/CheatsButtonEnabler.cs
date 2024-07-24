using UnityEngine;
using UnityEngine.UI;

public class CheatsButtonEnabler : MonoBehaviour
{
    public Button controlsButton;
    public Button cheatsButton;
    public Button miscButton;

    private void OnEnable()
    {
        bool isInMenu = GameUI.instance.isMenuScene;
        SetButtonEnabled(!isInMenu);
    }

    public void SetButtonEnabled(bool shouldEnable)
    {
        cheatsButton.interactable = shouldEnable;

        if (shouldEnable)
        {
            Navigation controlsNavigation = controlsButton.navigation;
            controlsNavigation.selectOnDown = cheatsButton;
            controlsButton.navigation = controlsNavigation;

            Navigation miscNavigation = miscButton.navigation;
            miscNavigation.selectOnUp = cheatsButton;
            miscButton.navigation = miscNavigation;
        }
        else
        {
            Navigation controlsNavigation = controlsButton.navigation;
            controlsNavigation.selectOnDown = miscButton;
            controlsButton.navigation = controlsNavigation;

            Navigation miscNavigation = miscButton.navigation;
            miscNavigation.selectOnUp = controlsButton;
            miscButton.navigation = miscNavigation;
        }
    }
}