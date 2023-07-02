using SliderVocalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        internal void PreRandomize(VocalizerParameters preset, VocalRandomizationContext context, T upcoming);

        float IVocalizer.RandomizeVocalization (VocalizerParameters preset, VocalRandomizationContext context)
        {
            float totalDuration = 0;
            if (Vocalizers == null)
            {
                Debug.LogWarning($"{ToString()} => vocalizers null");
                return 0;
            }
            for (int i = 0; i < Vocalizers.Count; i++)
            {
                var v = Vocalizers[i];
                PreRandomize(preset, context, v);
                totalDuration += v.RandomizeVocalization(preset, context);
            }
            return totalDuration;
        }

        IEnumerator IVocalizer.Vocalize(VocalizerParameters preset, VocalizationContext context, int idx, int lengthOfComposite)
        {
            yield return new WaitUntil(() => GetStatus() == VocalizerCompositeStatus.CanPlay);
            SetStatus(VocalizerCompositeStatus.Playing);
            if (Vocalizers == null)
            {
                Debug.LogWarning($"{ToString()} => vocalizers null");
                yield break;
            }
            for (int i = 0; i < Vocalizers.Count; i++)
            {
                var v = Vocalizers[i];
                SetCurrent(v);
                yield return v.Vocalize(preset, context, idx: i, lengthOfComposite: Vocalizers.Count);
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
        this T composite, VocalizerParameters preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        where T : IVocalizer
        => composite.Vocalize(preset, context, idx, lengthOfComposite);

    public static void Stop<T>(this T composite) where T : IVocalizer
    {
        composite.Stop();
        if (composite is VocalizableParagraph)
        {
            VocalizableParagraph.speakers.Remove(composite as VocalizableParagraph);
            AudioManager.StopDampen(composite as VocalizableParagraph);
        }
    }

    public static void ClearProgress<T>(this T composite)
         where T : IVocalizer
        => composite.ClearProgress();
}