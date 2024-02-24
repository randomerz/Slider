using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiGemDesertScene : MagiGem
{
    private const string DESERT_GEM_SAVE_STRING = "magiTechDesert";
    private const string HAS_GEM_TRANSPORTER = "MagitechHasGemTransporter";

    //fix edge cases for leaving scene
    public override void Save()
    {
        if (isTransporting)
        {
            // Just don't call the base one!
            // We already set the flag instantly so who cares
        }
    }

    public override void EnableGem()
    {
        gemItem.gameObject.SetActive(!SaveSystem.Current.GetBool(DESERT_GEM_SAVE_STRING));
    }

    public override void TransportGem()
    {
        // Do we care if you have the transporter or not? meh
        // if (!SaveSystem.Current.GetBool(HAS_GEM_TRANSPORTER)) return;
        SaveSystem.Current.SetBool(DESERT_GEM_SAVE_STRING, true);
        isTransporting = true;
        StartCoroutine(TransportGemVFXCoroutine());
    }

    protected override void FinishTransportGem()
    {
        // Do nothing!
    }
}
