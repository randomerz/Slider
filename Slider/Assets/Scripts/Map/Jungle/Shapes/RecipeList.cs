using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeList", menuName = "Scriptable Objects/RecipeList")]

public class RecipeList : ScriptableObject
{
    public List<Recipe> list = new List<Recipe>();
    private int index = -1;

/*    public IEnumerator GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        if (index < list.Count - 1)
        {
            ++index;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        index = -1;
    }

    public object Current
    {
        get
        {
            return list[index];
        }
    }*/

/*    public Recipe GetNext()
    {
        return list[index++];
    }

    public bool HasNext()
    {
        return index < list.Count;
    }*/
}