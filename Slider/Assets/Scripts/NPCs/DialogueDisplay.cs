using System;
using SliderVocalization;
using TMPro;
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
        if (ping.transform.localPosition == Vector3.zero)
        {
            ping.transform.position = new Vector2(transform.position.x, transform.position.y + 1);
        }
    }

    public void DisplaySentence(string originalMessage, string localizedMessage, NPCEmotes.Emotes emote)
    {
        CheckContrast();
        CheckSize();
        DeactivateMessagePing();
        canvas.SetActive(true);

        StopAllCoroutines();

        originalMessage = originalMessage.Replace('‘', '\'').Replace('’', '\'').Replace("…", "...");
        
        // message = ConvertVariablesToStrings(message);

        textSpecialText.StopEffects();
        textSpecialBG.StopEffects();

        string toVocalize;
        string typed;
        
        // if translation exists, vocalize the original english version and type out the localized version
        // shrink the font to 1/3 of original size due to a "Tiny" font being used for English
        if (localizedMessage != null)
        {
            toVocalize = textTyperText.ReplaceAndStripRichText(originalMessage);
            
            localizedMessage = localizedMessage.Replace('‘', '\'').Replace('’', '\'').Replace("…", "...");
            
            typed = textTyperText.StartTyping(localizedMessage);
            textTyperBG.StartTyping(localizedMessage);
        }
        else
        {
            typed = textTyperText.StartTyping(originalMessage);
            textTyperBG.StartTyping(originalMessage);

            toVocalize = typed;
        }
        
        // in rare cases NaN is used at first iteration and blocks dialogue typing
        
        textTyperText.SetTextSpeed(GameSettings.textSpeed);
        textTyperBG.SetTextSpeed(GameSettings.textSpeed);
        
        if (AudioManager.useVocalizer && useVocalizer)
        {
            float totalDuration = vocalizer.SetText(toVocalize, emote);

            textTyperText.SetTextSpeed(totalDuration / typed.Length);
            textTyperBG.SetTextSpeed(totalDuration / typed.Length);

            AudioManager.DampenMusic(this, 0.6f, totalDuration + 0.2f);

            if (vocalizer.GetVocalizationState() == VocalizerCompositeState.CanPlay)
            {
                vocalizer.StartReadAll(emote);
            }
        }
        
        // StartCoroutine(TypeSentence(message.ToCharArray()));
    }

    public void FadeAwayDialogue()
    {
        if (AudioManager.useVocalizer && useVocalizer)
        {
            vocalizer.Stop();
        }
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
        bool highContrastMode = SettingsManager.Setting<bool>(Settings.HighContrastTextEnabled).CurrentValue;
        highContrastBG.SetActive(highContrastMode);
    }

    public void SetFont(TMP_FontAsset font, float scale, bool clearWordSpacing)
    {
        textTyperText.SetFont(font, scale);
        textTyperBG.SetFont(font, scale);

        if (clearWordSpacing)
        {
            textTyperText.ClearWordSpacing();
        }
    }

    private void CheckSize()
    {
        bool doubleSizeMode = SettingsManager.Setting<bool>(Settings.BigTextEnabled).CurrentValue;
        canvas.transform.localScale = doubleSizeMode ? new Vector3(1.5f, 1.5f, 1.5f) : Vector3.one;
    }
}
