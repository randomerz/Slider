using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Scriptable Objects/Audio Modifier Preset")]
public class AudioModifier : ScriptableObject
{
    public AudioModifierProperty[] adjustments;

    [Serializable]
    public class AudioModifierProperty
    {
        [FMODUnity.ParamRef]
        public string parameter;
        public float value;
    }

    public ModifierType type;

    public enum ModifierType
    {
        IndoorMusic3Eq
    }
}
