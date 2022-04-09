using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClick : MonoBehaviour, IPointerEnterHandler
{
    Button button;
    [Tooltip("Set this true to make this the default button selected when the menu opens")]
    [SerializeField] private bool isSelectedOnMenuOpen;

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
        AudioManager.Play("UI Click");
    }

    // Player can use the mouse or movement keys to select buttons
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.Select();
    }
}
