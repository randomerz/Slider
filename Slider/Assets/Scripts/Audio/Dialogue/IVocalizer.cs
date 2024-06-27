using System.Collections;

namespace SliderVocalization
{
    public interface IVocalizer
    {
        /// <summary>
        /// Internally set the duration of the next vocalization, needs to be called on every vocalization.
        /// </summary>
        /// <returns>Duration of vocalization</returns>
        public float RandomizeVocalization(VocalizerParameters preset, VocalRandomizationContext context);

        public IEnumerator Vocalize(VocalizerParameters preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1);
        public void Stop();
        public bool IsEmpty { get; }
        public int Count<T>() where T: IVocalizer;
        public void ClearProgress();
    }
}
