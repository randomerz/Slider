using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SliderVocalization
{
    public interface IVocalizer
    {
        /// <summary>
        /// Internally set the duration of the next vocalization, needs to be called on every vocalization.
        /// </summary>
        /// <returns>Duration of vocalization</returns>
        public float RandomizeVocalization(VocalizerParameters parameters, VocalRandomizationContext context);

        public IEnumerator Vocalize(VocalizerParameters parameters, VocalizationContext context, int idx = 0, int lengthOfComposite = 1);
        public void Stop();
        public bool IsEmpty { get; }
        public void ClearProgress();
    }
}
