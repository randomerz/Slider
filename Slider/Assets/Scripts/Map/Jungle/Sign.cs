using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>();
    
    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }

        recievedShapes = new Dictionary<Path, Shape>();
        foreach (Direction d in paths.Keys)
        {
            recievedShapes.Add(paths[d], null);
        }
    }
    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
        SGrid.OnSTileEnabled += STileEnabled;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
        SGrid.OnSTileEnabled -= STileEnabled;
    }

    private void UpdateShapesOnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        recievedShapes = new Dictionary<Path, Shape>();
        foreach (Direction d in paths.Keys)
        {
            recievedShapes.Add(paths[d], null);
        }
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }
    }

    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        if (parents.Contains(this) && shape != null)
        {
            return;
        }

        if(shape != null)
        {
            print(shape.name);
        }

        parents.Add(this);

        if (path.pair != null)
        {
            recievedShapes[path.pair] = shape;
            MergeShapes();
            CreateShape(parents);
        }
        else
        {
            recievedShapes[path] = shape;
            MergeShapes();
            CreateShape(parents);
        }
    }
    public void MergeShapes()
    {
        List<Shape> shapesRecieved = new List<Shape>();

        foreach (Direction d in paths.Keys)
        {
            if (recievedShapes[paths[d]] != null)
            {
                // print(recievedShapes[paths[d]]);
                // print(d);
                shapesRecieved.Add(recievedShapes[paths[d]]);
            }
        }

/*        print(this.gameObject.name + " merging");
        foreach (Shape s in shapesRecieved)
        {
            print(s.name);
        }*/

        foreach (Recipe recipe in recipes.list)
        {
            //print(recipe.result.name);
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                currentShape = hold;
               // print("merged: " + currentShape.type);
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
