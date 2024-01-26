using System.Collections;
using System.Collections.Generic;
using Discord;
using UnityEngine;

public class JungleRecipeBookSave : Singleton<JungleRecipeBookSave>, ISavable
{
    public enum ShapeStatus {
        None,
        PendingOutline,
        Outline,
        PendingFull,
        Full,
    }

    public const string COMPLETION_STATUS_SAVE_PREFIX = "JungleCompletionStatus_";
    public const string NUMBER_CREATED_SAVE_PREFIX = "JungleNumberCreated_";
    public const string NUMBER_COMBINATION_CREATED_SAVE_PREFIX = "JungleNumberCombinationCreated_"; // JungleNumberCombinationCreated_Circle_0 = 10
                                                                                                    // JungleNumberCombinationCreated_{Result}_{Index} = Number

    private static readonly Dictionary<Shape, ShapeStatus> completionStatus = new();
    private static readonly Dictionary<Shape, int> numShapeCreated = new();

    [SerializeField] private List<Shape> allShapes = new();
    [SerializeField] private List<Shape> initialKnownShapes = new();
    [SerializeField] private RecipeList recipeList;

    private void Awake()
    {
        if (_instance == null)
        {
            InitializeSingleton();
        }

        if (allShapes.Count != recipeList.list.Count + 4)
        {
            Debug.LogWarning("Warning: Did you forget to add a shape from the recipe list to 'allShapes'?");
        }
    }

    public void Load(SaveProfile profile)
    {
        InitializeSingleton();

        foreach (Shape s in allShapes)
        {
            completionStatus[s] = (ShapeStatus)profile.GetInt(COMPLETION_STATUS_SAVE_PREFIX + s.shapeName);
            numShapeCreated[s] = profile.GetInt(NUMBER_CREATED_SAVE_PREFIX + s.shapeName);
        }

        foreach (Shape s in initialKnownShapes)
        {
            completionStatus[s] = ShapeStatus.Full;
            if (numShapeCreated[s] == 0)
            {
                IncrementNumberCreated(s);
            }
        }

        foreach (Recipe r in recipeList.list)
        {
            for (int i = 0; i < r.combinations.Count; i++)
            {
                r.combinations[i].numberOfTimesCreated = profile.GetInt(NUMBER_CREATED_SAVE_PREFIX + $"{r.result.shapeName}_{i}");
            }
        }
    }

    public void Save()
    {
        foreach (Shape s in completionStatus.Keys)
        {
            SaveSystem.Current.SetInt(COMPLETION_STATUS_SAVE_PREFIX + s.shapeName, (int)completionStatus[s]);
        }

        foreach (Shape s in numShapeCreated.Keys)
        {
            SaveSystem.Current.SetInt(NUMBER_CREATED_SAVE_PREFIX + s.shapeName, numShapeCreated[s]);
        }

        foreach (Recipe r in recipeList.list)
        {
            for (int i = 0; i < r.combinations.Count; i++)
            {
                SaveSystem.Current.SetInt(NUMBER_CREATED_SAVE_PREFIX + $"{r.result.shapeName}_{i}", r.combinations[i].numberOfTimesCreated);
            }
        }
    }

    public static ShapeStatus GetCompletionStatus(Shape shape)
    {
        if (!completionStatus.ContainsKey(shape))
        {
            Debug.LogWarning("Added a missing key: " + shape.shapeName);
            completionStatus[shape] = ShapeStatus.None;
        }
        
        return completionStatus[shape];
    }

    public static void SetCompletionStatus(Shape shape, ShapeStatus status)
    {
        completionStatus[shape] = status;
    }

    public static int GetNumberCreated(Shape shape)
    {
        if (!numShapeCreated.ContainsKey(shape))
        {
            Debug.LogWarning("Added a missing key: " + shape.shapeName);
            numShapeCreated[shape] = 0;
        }
        
        return numShapeCreated[shape];
    }

    public static void IncrementNumberCreated(Shape shape)
    {
        if (shape == null)
        {
            return;
        }

        if (!numShapeCreated.ContainsKey(shape))
        {
            Debug.LogWarning("Added a missing key: " + shape.shapeName);
            numShapeCreated[shape] = 0;
        }

        numShapeCreated[shape] += 1;
        if (numShapeCreated[shape] == 1)
        {
            CheckCompletionUpdates();
        }
    }

    private static void CheckCompletionUpdates()
    {
        foreach (Recipe r in _instance.recipeList.list)
        {
            Shape shape = r.result;
            if (numShapeCreated[shape] > 0)
            {
                completionStatus[shape] = ShapeStatus.Full;
            }
            else
            {
                // Debug.Log($"Checking {shape.shapeName} combinations...");
                foreach (Recipe.Shapes shapes in r.combinations)
                {
                    if (shapes.isSecondaryRecipe)
                        continue;
                    
                    if (ValidForOutline(shapes.ingredients))
                    {
                        completionStatus[shape] = ShapeStatus.Outline;
                    }
                }
            }
        }
    }

    private static bool ValidForOutline(List<Shape> shapes)
    {
        foreach (Shape s in shapes)
        {
            if (completionStatus[s] != ShapeStatus.Full && completionStatus[s] != ShapeStatus.PendingFull)
            {
                return false;
            }
        }

        return true;
    }

    public static bool AllRecipesCompleted()
    {
        foreach (Recipe r in _instance.recipeList.list)
        {
            Shape shape = r.result;
            foreach (Recipe.Shapes shapes in r.combinations)
            {
                if (shapes.isSecondaryRecipe)
                    continue;
                
                if (shapes.numberOfTimesCreated == 0)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}
