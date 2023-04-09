using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Composite instead of class due to possible multi-inheritance requirements. For example a vocalizer might need to be a Monobehaviour
/// </summary>
/// <typeparam name="T">Segments this composite is composed of</typeparam>
public interface IVocalizerComposite<T> : IVocalizer where T : IVocalizer
{
    List<T> Vocalizers { get; }
    T Current { get; protected set; }
    public VocalizerCompositeStatus Status { get; protected set; }

    IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, T prior, T upcoming, int upcomingIdx);
    IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, T completed, T upcoming, int upcomingIdx);

    IEnumerator IVocalizer.Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
    {
        yield return new WaitUntil(() => Status == VocalizerCompositeStatus.CanPlay);
        Status = VocalizerCompositeStatus.Playing;
        for (int i = 0; i < Vocalizers.Count; i++)
        {
            var v = Vocalizers[i];
            Current = v;
            yield return Prevocalize(preset, context, default, v, i);
            yield return v.Vocalize(preset, context, idx: i, lengthOfComposite: Vocalizers.Count);
            yield return Postvocalize(preset, context, v, default, i + 1);
            if (Status == VocalizerCompositeStatus.Stopping) break;
        }
        Status = VocalizerCompositeStatus.CanPlay;
    }

    void IVocalizer.Stop()
    {
        if (Status == VocalizerCompositeStatus.Playing)
        {
            Current?.Stop();
            ClearProgress();
            Status = VocalizerCompositeStatus.Stopping;
        }
    }

    void IVocalizer.ClearProgress()
    {
        foreach (T member in Vocalizers)
        {
            member.ClearProgress();
        }
    }
}

public enum VocalizerCompositeStatus
{
    CanPlay,
    Playing,
    Stopping
}