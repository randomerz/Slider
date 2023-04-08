using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVocalizer
{
    /// <summary>
    /// Implicitly updates the context
    /// </summary>
    public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context, int idx = 0, int lengthOfComposite = 1);
    public bool IsEmpty { get; }
}