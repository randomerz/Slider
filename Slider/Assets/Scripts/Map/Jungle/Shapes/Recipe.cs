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
            if (AllIngredients(shapes)) //not sure if this works since idk if order matters //IT MIGHT
            {
                return result;
            }
        }

        return null;
    }

    public bool AllIngredients(List<Shape> shapes)
    {
        foreach (Shapes combo in combinations)
        {
            List<Shape> ingredients = combo.ingredients;
            //check each combo!

            List<Shape> hold = new List<Shape>();
            foreach (Shape shape in shapes)
            {
                hold.Add(shape);
            }

            foreach (Shape ingredient in ingredients)
            {
                //check if they all have the same things
                bool removed = hold.Remove(ingredient);
                if (!removed)
                {
                    return false;
                }
            }
            if (hold.Count > 0)
            {
                return false;
            }
        }
        return false;
    }
}
