using System.Collections.Generic;
using Localization;
using UnityEngine;

public class MilitaryAllyDialogue : Singleton<MilitaryAllyDialogue>, IDialogueTableProvider
{
    public enum MilitaryAllyDialogueCode
    {
        Intro1,
        Intro2,
        MoveSuccess1,
        MoveSuccess2,
        MoveError1,
        MoveError2,
        MoveCancelled,
        MoveOffMap,
        MoveTooFar,
        MoveOccupied,
        MoveThroughWalls,
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<MilitaryAllyDialogueCode, string>
        {
            {
                MilitaryAllyDialogueCode.Intro1,
                "Use that flag to move us to adjacent tiles!"
            },
            {
                MilitaryAllyDialogueCode.Intro2,
                "Defend our land!"
            },
            {
                MilitaryAllyDialogueCode.MoveSuccess1,
                "Let's go, soldiers!"
            },
            {
                MilitaryAllyDialogueCode.MoveSuccess2,
                "Keep moving forward!"
            },
            {
                MilitaryAllyDialogueCode.MoveError1,
                "I am... dead??? Something went wrong!"
            },
            {
                MilitaryAllyDialogueCode.MoveError2,
                "Something went wrong!"
            },
            {
                MilitaryAllyDialogueCode.MoveCancelled,
                "Move was cancelled."
            },
            {
                MilitaryAllyDialogueCode.MoveOffMap,
                "We cannot leave the battlefield!"
            },
            {
                MilitaryAllyDialogueCode.MoveTooFar,
                "New location was not one tile away!"
            },
            {
                MilitaryAllyDialogueCode.MoveOccupied,
                "Can't move to an occupied tile!"
            },
            {
                MilitaryAllyDialogueCode.MoveThroughWalls,
                "We cannot move through walls!"
            },
        }
    );

    private void Awake()
    {
        InitializeSingleton();
    }

    public static string GetLocalizedOf(MilitaryAllyDialogueCode code)
    {
        return _instance.GetLocalized(code).TranslatedOrFallback;
    }
}