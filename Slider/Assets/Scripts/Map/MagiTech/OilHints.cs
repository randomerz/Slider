using UnityEngine;

// TODO: Localize
public class OilHints : MonoBehaviour
{
    public const string OIL_HINT_SAVE_STRING = "MagiTechOilHint";

    private void Start()
    {
        SaveSystem.Current.SetString(OIL_HINT_SAVE_STRING, "One barrel left!");
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
            hint = "Something went wrong!";
        }
        else if (!PlayerInventory.Contains("Oil #2", Area.MagiTech))
        {
            hint = "1 barrel left! It should be behind the rocket.";
        }
        else if (!PlayerInventory.Contains("Oil #3", Area.MagiTech))
        {
            hint = "1 barrel left! It should be on a rock in the past.";
        }
        else if (!PlayerInventory.Contains("Oil #4", Area.MagiTech))
        {
            hint = "1 barrel left! It should be on a rock in the present.";
        }
        else
        {
            hint = "Something went wrong!";
        }
        SaveSystem.Current.SetString(OIL_HINT_SAVE_STRING, hint);
    }
}