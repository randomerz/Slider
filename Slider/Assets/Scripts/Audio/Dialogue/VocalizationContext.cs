using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collection of parameters guiding a current vocalization instance. Shared between different levels of vocalization
/// </summary>
[System.Serializable]
public class VocalizationContext
{
    public Transform root;
    public VocalizationContext(Transform root)
    {
        this.root = root;
    }

    #region SENTENCE VOCALIZER RESPONSIBILITIES
    public float wordPitchBase;
    public float wordPitchIntonated;
    public bool isCurrentWordLow;
    #endregion

    #region WORD VOCALIZER RESPONSIBILITIES
    #endregion

    #region PHONEME CLUSTER VOCALIZER RESPONSIBILITIES
    public float vowelOpeness;
    public float vowelForwardness;
    #endregion
}
