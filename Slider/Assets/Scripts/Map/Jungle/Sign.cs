using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    private Dictionary<Path, Shape> shapes = new Dictionary<Path, Shape>(); //idk if i sshould rename this i should think
    public string startPath = "left";
    
    // Start is called before the first frame update
    void Awake()
    {
        print("creating sign");
        SetPaths();
/*        foreach (Path path in paths)
        {
            path.ChangePair();
        }*/

        if (shapes.Count == 0)
        {
            foreach (Path path in paths)
            {
                shapes.Add(path, null);
            }
        }

        if (stringToIndex.ContainsKey(startPath))
        {
            currentDirectionIndex = stringToIndex[startPath];
        }
    }
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
        shapes = new Dictionary<Path, Shape>();
        foreach (Path path in paths)
        {
            shapes.Add(path, null);
        }
    }


    public override void RecieveShape(Path path, Shape shape)
    {
        //somehow take in shapes and merge is needed
        // also be able to remove a shape when the box output diff stuff or the path stops
       // print("sign got shape");
        shapes[path.pair] = shape;
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

/*        print("boo");
        foreach (Shape shape in shapesRecieved)
        {
            print(shape);
        }
        print("ahdkfljaldf");*/
        
/*        while (recipes.HasNext())
        {
            //not finding the recipe :C
            Recipe recipe = recipes.GetNext();
            //print(recipe);
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                //we found the recipe
                //brah
                currentShape = hold;
                print(currentShape.type);
                return;
            }
        }
*/
        currentShape = shapesRecieved[0]; //assuming there is only one shape in the list B)
        //print("sign shape: " + currentShape.name);
    }
}
