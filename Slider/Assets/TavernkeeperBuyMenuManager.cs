using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// We need this to handle selecting a new button after a slider is purchased from the buy menu in the tavernkeeper UI.
/// </summary>
public class TavernkeeperBuyMenuManager : MonoBehaviour
{
    [Tooltip("Place all of the buy buttons here. They should be ordered from top to " +
        "bottom so that we select the uppermost one when buying a slider.")]
    [SerializeField] private Button[] buttons;

    /// <summary>
    /// Selects the first buy button in the list of buttons stored in this component.
    /// </summary>
    public void SelectUppermostBuyButton()
    {
        foreach (Button button in buttons)
        {
            if (button.gameObject.activeInHierarchy)
            {
                button.Select();
                break;
            }
        }
    }

    private void OnEnable()
    {
        SelectUppermostBuyButton();
    }
}
