using System.Collections.Generic;
using UnityEngine;

public class MilitaryHints : MonoBehaviour
{
    private const string HINT_SAVE_STRING = "MilitaryCommanderHint";
    public const string CYCLE_UNIT_SAVE_STRING = "MilitaryNumTimesCycledUnit";

    private const string HINT_DEFAULT = "Keep at it kiddo. You're lucky that last run was just a simulation.";
    private const string HINT_UNIT_SELECT = "Here's some advice: You can swap the unit you spawn. Use the flag before the computer.";
    private readonly List<string> HINTS = new() {
        "Here's some advice: You'll get a new supply drop every other tile.",
        "Recon is telling us that the aliens usually target the nearest robot. Ties broken randomly.",
        "There's 6 waves total. Each spawns when all the enemies are defeated.",
        "You don't have to use resupplies right away. It can be helpful to hold off until a more opportune moment.",
        "At the end of a wave, you might be able to find some time to move bad tiles out of the way.",
        // "You have one of those magic scrolls? You can try using that and it might get you out of a pinch.",
        "Do your best to keep your allies alive. Every unit matters in the end!",
    };

    private int nextHintIndex = 0;

    private void OnEnable()
    {
        MilitaryGrid.OnRestartMilitary += UpdateHint;
    }

    private void OnDisable()
    {
        MilitaryGrid.OnRestartMilitary -= UpdateHint;
    }

    private void Start()
    {
        SaveSystem.Current.SetString(HINT_SAVE_STRING, HINT_DEFAULT);
    }

    private void UpdateHint(object sender, System.EventArgs e) => UpdateHint();

    public void UpdateHint()
    {
        int numTimes = SaveSystem.Current.GetInt(CYCLE_UNIT_SAVE_STRING);
        if (numTimes == 0)
        {
            SaveSystem.Current.SetString(HINT_SAVE_STRING, HINT_UNIT_SELECT);
            return;
        }

        string hint = HINTS[nextHintIndex];
        SaveSystem.Current.SetString(HINT_SAVE_STRING, hint);
        nextHintIndex = (nextHintIndex + 1) % HINTS.Count;
    }
}