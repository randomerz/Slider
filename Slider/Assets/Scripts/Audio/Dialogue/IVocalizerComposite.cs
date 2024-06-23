using System;
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

        int IVocalizer.Count<V>()
        {
            if (this is V)
            {
                return 1;
            }
            
            int count = 0;
            foreach (var voc in Vocalizers)
            {
                count += voc.Count<V>();
            }
            return count;
        }

        internal void PreRandomize(VocalizerParameters preset, VocalRandomizationContext context, T upcoming);
        
        public VocalizerCompositeState GetVocalizationState();
        internal void SetVocalizationState(VocalizerCompositeState newState);

        public VocalizerCompositeState StateTransition(VocalizerCompositeState newState)
        {
            var oldState = GetVocalizationState();
            bool canSet;
            
            switch (newState)
            {
                case VocalizerCompositeState.CanPlay:
                    canSet = true;
                    break;
                case VocalizerCompositeState.Playing:
                    canSet = oldState == VocalizerCompositeState.CanPlay;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }

            if (canSet)
            {
                SetVocalizationState(newState);
                return newState;
            }
            else
            {
                return oldState;
            }
        }

        public WaitUntil WaitUntilCanPlay()
        {
            return new(() => GetVocalizationState() == VocalizerCompositeState.CanPlay);
        }

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
            StateTransition(VocalizerCompositeState.Playing);
            
            if (Vocalizers == null)
            {
                Debug.LogWarning($"{ToString()} => vocalizers null");
                yield break;
            }
            for (int i = 0; i < Vocalizers.Count; i++)
            {
                var v = Vocalizers[i];
                yield return v.Vocalize(preset, context, idx: i, lengthOfComposite: Vocalizers.Count);
            }

            StateTransition(VocalizerCompositeState.CanPlay);
        }

        void IVocalizer.Stop()
        {
            foreach (var voc in Vocalizers)
            {
                voc.Stop();
            }

            ClearProgress();
            
            if (this is MonoBehaviour self)
            {
                self.StopAllCoroutines();
            }

            StateTransition(VocalizerCompositeState.CanPlay);
        }

        void IVocalizer.ClearProgress()
        {
            foreach (T member in Vocalizers)
            {
                member.ClearProgress();
            }
        }
    }

    public enum VocalizerCompositeState
    {
        CanPlay,
        Playing
    }
}
public static class VocalizerCompositeExtensions
{
    public static int GetCount<T, V>(this IVocalizerComposite<T> composite) where T : IVocalizer where V: IVocalizer
        => composite.Count<V>();
    
    public static bool GetIsEmpty<T>(this IVocalizerComposite<T> composite) where T : IVocalizer
        => composite.IsEmpty;

    public static IEnumerator Vocalize<T>(this IVocalizerComposite<T> composite, VocalizerParameters preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1)
        where T : IVocalizer
        => composite.Vocalize(preset, context, idx, lengthOfComposite);

    public static void Stop<T>(this IVocalizerComposite<T> composite) 
        where T : IVocalizer
    {
        composite.Stop();
        if (composite is VocalizableParagraph paragraph)
        {
            VocalizableParagraph.speakers.Remove(paragraph);
            AudioManager.StopDampen(paragraph);
        }
    }

    public static void MarkAsStarted<T>(this IVocalizerComposite<T> composite) where T : IVocalizer =>
        composite.StateTransition(VocalizerCompositeState.Playing);

    public static void ClearProgress<T>(this IVocalizerComposite<T> composite)
         where T : IVocalizer
        => composite.ClearProgress();
}