using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueDisplay : MonoBehaviour
{
    public static bool doubleSizeMode = false;
    public static bool highContrastMode = false;

    public TMPTextTyper textTyperText;
    public TMPTextTyper textTyperBG;
    public TMPSpecialText textSpecialText;
    public TMPSpecialText textSpecialBG;

    public GameObject ping;
    public GameObject canvas;
    public GameObject highContrastBG;

    void Start()
    {
        ping.transform.position = new Vector2(transform.position.x, transform.position.y + 1);
    }

    public void DisplaySentence(string message)
    {
        CheckContrast();
        CheckSize();
        DeactivateMessagePing();
        canvas.SetActive(true);
        StopAllCoroutines();
        message = message.Replace('‘', '\'').Replace('’', '\'').Replace("…", "...");
        // message = ConvertVariablesToStrings(message);
        textSpecialText.StopEffects();
        textSpecialBG.StopEffects();
        textTyperText.StartTyping(message);
        textTyperBG.StartTyping(message);
        // StartCoroutine(TypeSentence(message.ToCharArray()));
    }

    public void FadeAwayDialogue()
    {
        canvas.SetActive(false);
    }

    public void ActivateMessagePing()
    {
        ping.SetActive(true);
    }

    public void DeactivateMessagePing()
    {
        ping.SetActive(false);
    }

    public void SetMessagePing(bool value)
    {
        ping.SetActive(value);
    }

    private void CheckContrast()
    {
        highContrastBG.SetActive(highContrastMode);
    }

    private void CheckSize()
    {
        if (doubleSizeMode)
        {
            canvas.transform.localScale = new Vector3(2, 2, 2);
        }
        else
        {
            canvas.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
