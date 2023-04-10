using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliderVocalization
{
    public interface IVocalizer
    {
        /// <summary>
        /// Implicitly updates the context
        /// </summary>
        public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1);
        public void Stop();
        public bool IsEmpty { get; }
        public void ClearProgress();
    }
}
