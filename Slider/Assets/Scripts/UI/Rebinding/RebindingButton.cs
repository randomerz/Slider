using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RebindingButton : MonoBehaviour
{
    [SerializeField] private Control control;
    [SerializeField] private TextMeshProUGUI bindingText;

    private void Awake()
    {
        InputRebinding.OnRebindCompleted += UpdateText;
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void OnClick()
    {
        InputRebinding.StartInteractiveRebindOperation(control);
    }

    private void UpdateText()
    {
        bindingText.text = GetBindingDisplayStringForControl(control);
    }

    private string GetBindingDisplayStringForControl(Control control)
    {
        return Controls.BindingDisplayString(control)
                       .ToUpper()
                       .Replace("PRESS ", "")
                       .Replace(" ARROW", "");
    }
}
