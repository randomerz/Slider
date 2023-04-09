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
    public PlayStatus Status { get; protected set; }

    IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, T prior, T upcoming, int upcomingIdx);
    IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, T completed, T upcoming, int upcomingIdx);

    IEnumerator IVocalizer.Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
    {
        yield return new WaitUntil(() => Status == PlayStatus.CanPlay);
        Status = PlayStatus.Playing;
        for (int i = 0; i < Vocalizers.Count; i++)
        {
            var v = Vocalizers[i];
            Current = v;
            yield return Prevocalize(preset, context, default, v, i);
            yield return v.Vocalize(preset, context, idx: i, lengthOfComposite: Vocalizers.Count);
            yield return Postvocalize(preset, context, v, default, i + 1);
            if (Status == PlayStatus.Stopping) break;
        }
        Status = PlayStatus.CanPlay;
    }

    void IVocalizer.Stop()
    {
        if (Status == PlayStatus.Playing)
        {
            Current?.Stop();
            ClearProgress();
            Status = PlayStatus.Stopping;
        }
    }

    void IVocalizer.ClearProgress()
    {
        foreach (T member in Vocalizers)
        {
            member.ClearProgress();
        }
    }

    public enum PlayStatus
    {
        CanPlay,
        Playing,
        Stopping
    }
}