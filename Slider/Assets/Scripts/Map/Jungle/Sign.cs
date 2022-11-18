using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    public Dictionary<Path, Shape> shapes = new Dictionary<Path, Shape>(); //idk if i sshould rename this i should think
    
    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }

        if (shapes.Count == 0)
        {
            foreach (Direction d in paths.Keys)
            {
                shapes.Add(paths[d], null);
            }
        }
    }
    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
    }

    private void OnDisable()
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


    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        //print("sign got shape " + shape);

        if (parents.Contains(this) && shape != null)
        {
            return;
        }

        parents.Add(this);

        if (path.pair != null)
        {
            shapes[path.pair] = shape;
            MergeShapes();
            CreateShape(parents);
        }
        else
        {
            shapes[path] = shape;
            MergeShapes();
            CreateShape(parents);
        }
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

        foreach (Shape s in shapesRecieved)
        {
            print(s.name);
        }

        foreach (Recipe recipe in recipes.list)
        {
            //print(recipe.result.name);
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                currentShape = hold;
                //print("merged: " + currentShape.type);
                return;
            }
        }

        if (shapesRecieved.Count == 0)
        {
            currentShape = null;
        }
        else
        {
            currentShape = shapesRecieved[0];
        }
    }
}
