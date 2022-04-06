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
        if (isSelectedOnMenuOpen)
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            button.Select();
        }
    }

    public void Click()
    {
        AudioManager.Play("UI Click");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        button.Select();
    }
}
