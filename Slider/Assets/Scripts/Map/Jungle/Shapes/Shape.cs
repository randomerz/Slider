using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Shape", menuName = "Scriptable Objects/Shape")]
public class Shape : ScriptableObject
{
    public string type;
    public Sprite sprite;
}
