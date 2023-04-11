using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paragraph = SliderVocalization.VocalizableParagraph;

public class DialogueDisplay : MonoBehaviour
{
    public bool useVocalizer;
    public Paragraph vocalizer;

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

        string parsed = textTyperText.StartTyping(message);
        textTyperBG.StartTyping(message);
        vocalizer.SetText(parsed);
        vocalizer.StartReadAll();
        // StartCoroutine(TypeSentence(message.ToCharArray()));
    }

    public void FadeAwayDialogue()
    {
        if (useVocalizer)
        {
            StartCoroutine(FadeWhenReady());
        }
        else
        {
            canvas.SetActive(false);
        }
    }

    IEnumerator FadeWhenReady()
    {
        vocalizer.Stop();
        yield return vocalizer.WaitUntilCanPlay();
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
        bool highContrastMode = SettingsManager.HighContrastTextEnabled;
        highContrastBG.SetActive(highContrastMode);
    }

    private void CheckSize()
    {
        bool doubleSizeMode = SettingsManager.BigTextEnabled;
        canvas.transform.localScale = doubleSizeMode ? new Vector3(1.5f, 1.5f, 1.5f) : Vector3.one;
    }
}
