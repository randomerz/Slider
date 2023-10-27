using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
        // Disallow starting keyboard rebind operations with controller
        if (WasPressedUsingController())
        {
            return;
        }
        InputRebinding.StartInteractiveRebindOperation(control);
    }

    private void UpdateText()
    {
        bindingText.text = GetBindingDisplayStringForControl(control);
    }

    private string GetBindingDisplayStringForControl(Control control)
    {
        return Controls.BindingDisplayString(control, forSpecificScheme: Controls.CONTROL_SCHEME_KEYBOARD_MOUSE)
                       .ToUpper()
                       .Replace("PRESS ", "")
                       .Replace(" ARROW", "");
    }

    private bool WasPressedUsingController()
    {
        return Gamepad.current != null && Gamepad.current.buttonSouth.IsPressed();
    }
}
