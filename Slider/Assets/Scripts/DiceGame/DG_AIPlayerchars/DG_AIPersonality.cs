using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DG_AIPersonality", menuName = "Scriptable Objects/Dice Game Ai Personality")]
public class DG_AIPersonality : ScriptableObject
{
    public string playerName;

    public Sprite CharacterSprite;
    public Sprite CharacterIconSprite;

    [Tooltip("Between 1 - 3")]
    [Min(0)]
    public int Intelligence;
    [Tooltip("Between 1 - 3")]
    [Min(0)]
    public int Aggressiveness;
}
