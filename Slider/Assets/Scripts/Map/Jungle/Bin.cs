using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : MonoBehaviour
{
    List<Recipe> recipes = new List<Recipe>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RecieveShape(Shape shape)
    {
        if (shape == null)
        {
            print("Pushed null shape");
        }
        else
        {
            //broadcast the shape has been made
            print("bin: " + shape.name);
        }
    }
}
