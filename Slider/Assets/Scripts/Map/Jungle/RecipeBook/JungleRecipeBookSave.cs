using System.Collections;
using System.Collections.Generic;
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

    private static Dictionary<Shape, ShapeStatus> completionStatus = new();
    private static Dictionary<Shape, int> numberCreated = new();

    [SerializeField] private List<Shape> allShapes = new();
    [SerializeField] private RecipeList recipeList;

    private void Awake()
    {
        InitializeSingleton();
    }

    public void Load(SaveProfile profile)
    {
        foreach (Shape s in allShapes)
        {
            completionStatus[s] = (ShapeStatus)profile.GetInt(COMPLETION_STATUS_SAVE_PREFIX + s.shapeName);
            numberCreated[s] = profile.GetInt(NUMBER_CREATED_SAVE_PREFIX + s.shapeName);
        }
    }

    public void Save()
    {
        foreach (Shape s in completionStatus.Keys)
        {
            SaveSystem.Current.SetInt(COMPLETION_STATUS_SAVE_PREFIX + s.shapeName, (int)completionStatus[s]);
        }

        foreach (Shape s in numberCreated.Keys)
        {
            SaveSystem.Current.SetInt(NUMBER_CREATED_SAVE_PREFIX + s.shapeName, numberCreated[s]);
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
        if (!numberCreated.ContainsKey(shape))
        {
            Debug.LogWarning("Added a missing key: " + shape.shapeName);
            numberCreated[shape] = 0;
        }
        
        return numberCreated[shape];
    }

    public static void IncrementNumberCreated(Shape shape)
    {
        numberCreated[shape] += 1;
        if (numberCreated[shape] == 1)
        {
            CheckCompletionUpdates(shape);
        }
    }

    private static void CheckCompletionUpdates(Shape shape)
    {
        if (!completionStatus.ContainsKey(shape))
        {
            Debug.LogWarning("Added a missing key: " + shape.shapeName);
            completionStatus[shape] = ShapeStatus.None;
        }

        if (!numberCreated.ContainsKey(shape))
        {
            Debug.LogWarning("Added a missing key: " + shape.shapeName);
            numberCreated[shape] = 0;
        }


        if (numberCreated[shape] > 0)
        {
            completionStatus[shape] = ShapeStatus.Full;
            return;
        }

        foreach (Recipe r in _instance.recipeList.list)
        {
            if (r.result == shape)
            {
                foreach (Recipe.Shapes shapes in r.combinations)
                {
                    if (shapes.isSecondaryRecipe)
                        continue;
                    
                    if (ValidForOutline(shapes.ingredients))
                    {
                        completionStatus[shape] = ShapeStatus.Outline;
                        break;
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
                return false;
        }

        return false;
    }

}
