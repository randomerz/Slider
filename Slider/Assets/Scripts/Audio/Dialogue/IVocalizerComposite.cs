using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVocalizerComposite<T> : IVocalizer where T : IVocalizer
{
    List<T> Vocalizers { get; }
    IEnumerator Prevocalize(VocalizerPreset preset, T prior, T upcoming);
    IEnumerator Postvocalize(VocalizerPreset preset, T completed, T upcoming);

    IEnumerator IVocalizer.Vocalize(VocalizerPreset preset, VocalizationContext context)
    {
        for (int i = 0; i < Vocalizers.Count; i++)
        {
            var v = Vocalizers[i];
            if (i != 0)
            {
                yield return Prevocalize(preset, Vocalizers[i - 1], v);
            }
            Debug.Log($"Vocalizing: {v}");
            (v as IVocalizer).Vocalize(preset, context);
            if (i != Vocalizers.Count - 1)
            {
                yield return Postvocalize(preset, v, Vocalizers[i + 1]);
            }
        }
    }
}