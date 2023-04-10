using SliderVocalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace SliderVocalization
{
    /// <summary>
    /// Composite instead of class due to possible multi-inheritance requirements. For example a vocalizer might need to be a Monobehaviour
    /// </summary>
    /// <typeparam name="T">Segments this composite is composed of</typeparam>
    public interface IVocalizerComposite<T> : IVocalizer where T : IVocalizer
    {
        public List<T> Vocalizers { get; }

        bool IVocalizer.IsEmpty => Vocalizers.Count == 0;

        public T GetCurrent();
        protected void SetCurrent(T value);
        public VocalizerCompositeStatus GetStatus();
        protected void SetStatus(VocalizerCompositeStatus value);

        internal IEnumerator Prevocalize(VocalizerPreset preset, VocalizationContext context, T prior, T upcoming, int upcomingIdx);
        internal IEnumerator Postvocalize(VocalizerPreset preset, VocalizationContext context, T completed, T upcoming, int upcomingIdx);

        IEnumerator IVocalizer.Vocalize(VocalizerPreset preset, VocalizationContext context, int idx, int lengthOfComposite)
        {
            yield return new WaitUntil(() => GetStatus() == VocalizerCompositeStatus.CanPlay);
            SetStatus(VocalizerCompositeStatus.Playing);
            for (int i = 0; i < Vocalizers.Count; i++)
            {
                var v = Vocalizers[i];
                SetCurrent(v);
                yield return Prevocalize(preset, context, default, v, i);
                yield return v.Vocalize(preset, context, idx: i, lengthOfComposite: Vocalizers.Count);
                yield return Postvocalize(preset, context, v, default, i + 1);
                if (GetStatus() == VocalizerCompositeStatus.Stopping) break;
            }
            SetStatus(VocalizerCompositeStatus.CanPlay);
        }

        void IVocalizer.Stop()
        {
            if (GetStatus() == VocalizerCompositeStatus.Playing)
            {
                GetCurrent()?.Stop();
                ClearProgress();
                SetStatus(VocalizerCompositeStatus.Stopping);
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
}
public static class VocalizerCompositeExtensions
{
    public static bool GetIsEmpty<T>(this T composite) where T : IVocalizer => (composite as IVocalizer).IsEmpty;

    public static IEnumerator Vocalize<T>(
        this T composite, VocalizerPreset preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        where T : IVocalizer
        => composite.Vocalize(preset, context, idx, lengthOfComposite);

    public static void Stop<T>(this T composite)
         where T : IVocalizer
        => composite.Stop();

    public static void ClearProgress<T>(this T composite)
         where T : IVocalizer
        => composite.ClearProgress();
}