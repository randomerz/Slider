using System.Collections.Generic;
using Localization;
using UnityEngine;
using TMPro;

public class ShopCreditsText : MonoBehaviour, IDialogueTableProvider 
{
    public ShopManager shopManager;
    public TextMeshProUGUI creditsText;

    private void Update()
    {
        creditsText.text = IDialogueTableProvider.Interpolate(
            this.GetLocalizedSingle("Credits"), new Dictionary<string, string>
            {
                { "credits", shopManager.GetCredits().ToString() }
            });
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<string, string>
        {
            { "Credits", "<credits/> Credits" }
        });
}