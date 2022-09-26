using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : MonoBehaviour
{
    List<Recipe> recipes = new List<Recipe>();
    Shape currentShape = null;

    private new void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += OnSTileMoveEarly;
    }

    private new void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveEarly;
    }

    private void OnSTileMoveEarly(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //remove all shapes
        currentShape = null;
        print("no shape");
    }

    public void RecieveShape(Shape shape)
    {
        if (shape == null)
        {
            print("no shape");
        }
        else
        {
            //broadcast the shape has been made
            print("bin: " + shape.name);
            currentShape =  shape;
        }
    }
}
