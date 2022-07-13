using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftButton : KeyboardButton
{
    public List<KeyboardButton> keyboardButtons;
    private bool isCapital = false;
    public void ToggleCapitalize()
    {
        if (!isCapital)
        {
            foreach (KeyboardButton b in keyboardButtons)
            {
                b.Capitalize(true);
            }
            isCapital = true;
        }
        else
        {
            foreach (KeyboardButton b in keyboardButtons)
            {
                b.Capitalize(false);
            }
            isCapital = false;
        }
    }
}
