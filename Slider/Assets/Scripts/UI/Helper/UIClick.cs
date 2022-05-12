using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Contains functionality for playing sound OnClick and selecting the button OnPointerEnter or OnEnable.
/// </summary>
public class UIClick : MonoBehaviour, IPointerEnterHandler
{
    Button button;

    [Tooltip("Set this true to make this the default button selected when the menu opens")]
    [SerializeField] private bool isSelectedOnMenuOpen;
    [Tooltip("Set this true to make this button selected when the pointer moves over it. " +
        "This means that the button will be triggered if PlayerAction is pressed while the cursor it")]
    [SerializeField] private bool selectOnPointerEnter;

    [Tooltip("Set this true to make the button not stay selected after being clicked. Useful if you" +
        "don't want PlayerAction to trigger this button.")]
    [SerializeField] private bool deselectOnClick;

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        StartCoroutine(IOnEnable());
    }

    /// <summary>
    /// We use this to wait one frame after opening the UI so we don't immediately interact with the button 
    /// on opening since Player Action is used for opening the UI and pressing buttons.
    /// This also handles selecting the button when the menu is opened if isSelectedOnMenuOpen is true.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IOnEnable()
    {
        // Apparently we have this script on some things without a button component, so let's avoid
        // scary NREs in the console
        if (button != null)
        {
            button.enabled = false;

            yield return new WaitForEndOfFrame();

            if (isSelectedOnMenuOpen)
            {
                button.Select();
            }

            button.enabled = true;
        }
    }

    public void Click()
    {
        if (deselectOnClick)
        {
            UINavigationManager.ClearSelectable();
        }
        AudioManager.Play("UI Click");
    }

    // Player can use the mouse or movement keys to select buttons
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectOnPointerEnter)
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            button.Select();
        }
    }
}
