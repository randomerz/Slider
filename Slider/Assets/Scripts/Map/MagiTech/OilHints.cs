using System.Collections.Generic;
using Localization;
using UnityEngine;

public class OilHints : MonoBehaviour, IDialogueTableProvider
{
    public const string OIL_HINT_SAVE_STRING = "MagiTechOilHint";

    private enum OilDialogueCode
    {
        HintError,
        HintNumber2,
        HintNumber3,
        HintNumber4,
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<OilDialogueCode, string>
        {
            {
                OilDialogueCode.HintError,
                "Something went wrong!"
            },
            {
                OilDialogueCode.HintNumber2,
                "1 barrel left! It should be behind the rocket."
            },
            {
                OilDialogueCode.HintNumber3,
                "1 barrel left! It should be on a rock in the past."
            },
            {
                OilDialogueCode.HintNumber4,
                "1 barrel left! It should be on a rock in the present."
            },
        }
    );

    private void Start()
    {
        SaveSystem.Current.SetString(OIL_HINT_SAVE_STRING, this.GetLocalized(OilDialogueCode.HintError).translated);
    }

    private void Update()
    {
        UpdateOilHint();
    }

    private void UpdateOilHint()
    {
        string hint;
        if (!PlayerInventory.Contains("Oil #1", Area.MagiTech))
        {
            hint = this.GetLocalized(OilDialogueCode.HintError).translated;
        }
        else if (!PlayerInventory.Contains("Oil #2", Area.MagiTech))
        {
            hint = this.GetLocalized(OilDialogueCode.HintNumber2).translated;
        }
        else if (!PlayerInventory.Contains("Oil #3", Area.MagiTech))
        {
            hint = this.GetLocalized(OilDialogueCode.HintNumber3).translated;
        }
        else if (!PlayerInventory.Contains("Oil #4", Area.MagiTech))
        {
            hint = this.GetLocalized(OilDialogueCode.HintNumber4).translated;
        }
        else
        {
            hint = this.GetLocalized(OilDialogueCode.HintError).translated;
        }
        SaveSystem.Current.SetString(OIL_HINT_SAVE_STRING, hint);
    }
}