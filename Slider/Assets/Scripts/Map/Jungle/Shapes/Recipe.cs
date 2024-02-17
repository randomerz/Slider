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
        public bool isSecondaryRecipe;
        [HideInInspector] public int numberOfTimesCreated; // Serialized in JungleRecipeBookSave.cs
    }

    public enum Difficulty
    {
        easy,
        medium_placeholder,
        hard,
    }

    public Shape result;
    public List<Shapes> combinations = new List<Shapes>();
    public Difficulty difficulty = Difficulty.hard;

    public Shape Check(List<Shape> shapes)
    {
        foreach (Shapes combo in combinations)
        {
            if (AllIngredients(shapes, combo)) 
            {
                combo.numberOfTimesCreated += 1;
                return result;
            }
        }

        return null;
    }

    public bool AllIngredients(List<Shape> shapes, Shapes combo)
    {
        // check each combo to see if they are the shapes being passed in
        List<Shape> ingredients = combo.ingredients;

        if (shapes.Count != ingredients.Count)
        {
            return false;
        }

        List<Shape> hold = new List<Shape>();


        foreach (Shape shape in shapes)
        {
            hold.Add(shape);
        }

        // check if they all have the same things
        foreach (Shape ingredient in ingredients)
        {
            bool removed = hold.Remove(ingredient);
            if (!removed)
            {
                return false;
            }
        }

        if (hold.Count == 0)
        {
            return true;
        }

        return false;
    }
}
