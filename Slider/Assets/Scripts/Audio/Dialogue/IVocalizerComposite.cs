using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVocalizerComposite<T> : IVocalizer where T : IVocalizer
{
    List<T> Vocalizers { get; }
    IEnumerator Prevocalize(T prior, T upcoming);
    IEnumerator Postvocalize(T completed, T upcoming);

    IEnumerator IVocalizer.Vocalize(VocalizerPreset preset, VocalizationContext context)
    {
        for (int i = 0; i < Vocalizers.Count; i++)
        {
            var v = Vocalizers[i];
            if (i != 0)
            {
                yield return Prevocalize(Vocalizers[i - 1], v);
            }
            v.Vocalize(preset, context);
            if (i != Vocalizers.Count - 1)
            {
                yield return Postvocalize(v, Vocalizers[i + 1]);
            }
        }
    }
}