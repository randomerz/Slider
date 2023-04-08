using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVocalizerComposite<T> : IVocalizer where T : IVocalizer
{
    List<T> Vocalizers { get; }
    IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, T prior, T upcoming, int upcomingIdx);
    IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, T completed, T upcoming, int upcomingIdx);

    IEnumerator IVocalizer.Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
    {
        for (int i = 0; i < Vocalizers.Count; i++)
        {
            var v = Vocalizers[i];
            yield return Prevocalize(preset, context, default, v, i);
            yield return v.Vocalize(preset, context, idx: i, lengthOfComposite: Vocalizers.Count);
            yield return Postvocalize(preset, context, v, default, i + 1);
        }
    }

#if UNITY_EDITOR
    void ClearProgress();
#endif
}