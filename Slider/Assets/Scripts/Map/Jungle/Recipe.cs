using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe : ScriptableObject
{
    Shape result;
    List<Shape> ingredients = new List<Shape>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Shape Check(List<Shape> shapes)
    {
        if (shapes.Equals(ingredients))
        {
            return result;
        }

        return null;
    }
}
