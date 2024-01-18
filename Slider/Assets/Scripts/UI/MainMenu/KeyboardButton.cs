using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardButton : MonoBehaviour
{
    public Button button;
    public TMP_InputField inputField;
    public TextMeshProUGUI character;

    private const int MAX_PROFILE_NAME_LENGTH = 10;

    public void Click()
    {
        if(MainMenuManager.KeyboardEnabled) 
        {
            AudioManager.Play("UI Click");
            if(inputField.text.Length < MAX_PROFILE_NAME_LENGTH)
                inputField.text += character.text;
        }
    }

    public void DeleteClick()
    {
        if (inputField.text.Length  > 0 && MainMenuManager.KeyboardEnabled)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
        AudioManager.Play("UI Click");
    }
    public void Capitalize(bool b)
    {
        if (b)
        {
            character.text = character.text.ToUpper();
        }
        else
        {
            character.text = character.text.ToLower();
        }
    }
}
