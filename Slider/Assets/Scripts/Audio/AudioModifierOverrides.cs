using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AudioModifierOverrides : MonoBehaviour
{
    [SerializeField]
    AudioModifier[] modifierOverrides;
    Dictionary<AudioModifier.ModifierType, AudioModifier> modifiers;

    public AudioModifier GetOverride (AudioModifier original)
    {
        if (original == null) { return null; }
        else
        {
            var overrideModifier = this[original.type];
            return overrideModifier == null ? original : overrideModifier;
        }
    }

    // technically using a default reference is the way to go, but an optional makes things clearer
    public AudioModifier this[AudioModifier.ModifierType t]
    {
        get
        {
            if (modifiers == null)
            {
                modifiers = new Dictionary<AudioModifier.ModifierType, AudioModifier>(modifierOverrides.Length);
                foreach(var m in modifierOverrides)
                {
                    if (modifiers.ContainsKey(m.type))
                    {
                        Debug.LogWarning($"{m.name} cannot be added to the overrides pool");
                    } else
                    {
                        modifiers.Add(m.type, m);
                    }
                }
            }

            return modifiers.ContainsKey(t) ? modifiers[t] : null;
        }
    }
}
