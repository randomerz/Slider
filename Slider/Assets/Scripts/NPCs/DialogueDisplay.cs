using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueDisplay : MonoBehaviour
{
    public TMPTextTyper textTyperText;
    public TMPTextTyper textTyperBG;
    public TMPSpecialText textSpecialText;
    public TMPSpecialText textSpecialBG;


    // public TextMeshProUGUI dialogueText;
    // public TextMeshProUGUI dialogueBG;
    public GameObject ping;

    public GameObject canvas;
    public GameObject highContrastBG;

    public static bool doubleSizeMode = false;
    public static bool highContrastMode = false;

    void Start()
    {
        ping.transform.position = new Vector2(transform.position.x, transform.position.y + 1);
    }

    public void DisplaySentence(string message)
    {
        CheckContrast();
        CheckSize();
        ReadMessagePing();
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

    private string ConvertVariablesToStrings(string message)
    {
        int startIndex = 0;
        while (message.IndexOf('<', startIndex) != -1)
        {
            startIndex = message.IndexOf('<', startIndex);
            // case with \<
            if (startIndex != 0 && message[startIndex - 1] == '\\')
            {
                // continue
                startIndex += 1;
                continue;
            }

            int endIndex = message.IndexOf('>', startIndex);
            if (endIndex == -1)
            {
                // no more ends!
                break;
            }
            string varName = message.Substring(startIndex + 1, endIndex - startIndex - 1);
            string varResult = SaveSystem.Current.GetString(varName);
            message = message.Substring(0, startIndex) + varResult + message.Substring(endIndex + 1);
            // startIndex = endIndex;
        }

        return message;
    }

    // IEnumerator TypeSentence(char[] charArray)
    // {
    //     dialogueText.text = "";
    //     dialogueBG.text = "";
    //     foreach (char letter in charArray)
    //     {
    //         dialogueText.text += letter;
    //         dialogueBG.text += letter;

    //         if (GameSettings.punctuation.IndexOf(letter) != -1)
    //             yield return new WaitForSeconds(GameSettings.textSpeed);

    //         yield return new WaitForSeconds(GameSettings.textSpeed);
    //     }
    // }

    public void FadeAwayDialogue()
    {
        canvas.SetActive(false);
    }

    public void NewMessagePing()
    {
        ping.SetActive(true);
    }

    public void ReadMessagePing()
    {
        ping.SetActive(false);
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
