using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class Recipe : ScriptableObject
{
    [System.Serializable]
    public class Shapes
    {
        public List<Shape> ingredients;
    }
    public  Shape result;
    public List<Shapes> combinations = new List<Shapes>();

    public Shape Check(List<Shape> shapes)
    {
        foreach (Shapes stuff in combinations) {
            if (shapes.Equals(stuff.ingredients)) //not sure if this works since idk if order matters
            {
                return result;
            }
        }

        return null;
    }
}
