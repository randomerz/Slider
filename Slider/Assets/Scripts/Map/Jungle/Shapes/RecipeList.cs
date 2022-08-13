using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeList", menuName = "Scriptable Objects/RecipeList")]

public class RecipeList : MonoBehaviour
{
    public List<Recipe> list = new List<Recipe>();
    private int index = 0;

    public Recipe GetNext()
    {
        return list[index++];
    }

    public bool HasNext()
    {
        return index < list.Count;
    }
}