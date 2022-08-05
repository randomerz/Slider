using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Box
{
    List<Recipe> recipes = new List<Recipe>();
    Dictionary<Path, Shape> shapes = new Dictionary<Path, Shape>(); //idk if i sshould rename this i should think
    
    // Start is called before the first frame update
    void Start()
    {
        if (left != null)
        {
            directions.Add(new Vector2(-1, 0));
            paths.Add(left);
        }
        if (right != null)
        {
            directions.Add(new Vector2(1, 0));
            paths.Add(right);
        }
        if (top != null)
        {
            directions.Add(new Vector2(0, 1));
            paths.Add(top);
        }
        if (bottom != null)
        {
            directions.Add(new Vector2(0, -1));
            paths.Add(bottom);
        }

        foreach (Path path in paths)
        {
            shapes.Add(path, null);
        }
    }

    // Update is called once per frame

    public override void RecieveShape(Path path, Shape shape)
    {
        //somehow take in shapes and merge is needed
        // also be able to remove a shape when the box output diff stuff or the path stops
        print("sign got shape");
        shapes[path] = shape;
        MergeShapes();
        CreateShape();
    }
    public void MergeShapes()
    {
        List<Shape> shapesRecieved = new List<Shape>(); 
        foreach (Path path in paths)
        {
            if (shapes[path] != null)
            {
                shapesRecieved.Add(shapes[path]);
            }
        }

        foreach (Recipe recipe in  recipes)
        {
            if (recipe.Check(shapesRecieved) != null)
            {
                //we found the recipe
                currentShape = recipe.Check(shapesRecieved);
                return;
            }
        }
        currentShape = shapesRecieved[0]; //supposed to just pass a shape
    }
}
