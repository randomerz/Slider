using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVocalizer
{
    /// <summary>
    /// Implicitly updates the context
    /// </summary>
    public IEnumerator Vocalize(VocalizerPreset preset, VocalizationContext context);
    public bool Vocalizable { get; }
}