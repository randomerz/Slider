using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    private Dictionary<Path, Shape> shapes = new Dictionary<Path, Shape>(); //idk if i sshould rename this i should think
    
    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();

        if (shapes.Count == 0)
        {
            foreach (Direction d in paths.Keys)
            {
                shapes.Add(paths[d], null);
            }
        }
    }
    private new void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
    }

    private new void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
    }

    private void UpdateShapesOnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }
        //remove all shapes
        shapes = new Dictionary<Path, Shape>();
        foreach (Direction d in paths.Keys)
        {
            shapes.Add(paths[d], null);
        }
    }


    public override void RecieveShape(Path path, Shape shape)
    {
        //somehow take in shapes and merge is needed
        // also be able to remove a shape when the box output diff stuff or the path stops
        // print("sign got shape");

        //sometimes this is like null because the path pairs havent been updated yet

        //print(path.gameObject.name);
        shapes[path.pair] = shape;
        MergeShapes();
        CreateShape();
    }
    public void MergeShapes()
    {
        List<Shape> shapesRecieved = new List<Shape>(); 
        foreach (Direction d in paths.Keys)
        {
            if (shapes[paths[d]] != null)
            {
                shapesRecieved.Add(shapes[paths[d]]);
            }
        }

/*        print("boo");
        foreach (Shape shape in shapesRecieved)
        {
            print(shape);
        }
        print("ahdkfljaldf");*/

        foreach (Recipe recipe in recipes.list)
        {
            //print(recipe.result.name);
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                currentShape = hold;
                print("merged: " + currentShape.type);
                return;
            }
        }

        currentShape = shapesRecieved[0];
    }
}
