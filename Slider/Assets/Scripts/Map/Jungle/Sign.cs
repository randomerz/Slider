using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>();

    public Shape question;
    
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

    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        if (parents.Contains(this))
        {
            path.Deactivate();
            return;
        }

        if (path.pair != null)
        {
            recievedShapes[path.pair] = shape;
        }
        else
        {
            recievedShapes[path] = shape;
        }

        
        MergeShapes();

        if (shape != null) {
            currParents.AddRange(parents);
        } else {
            if (currentShape == null) {
                currParents = parents;
            } else {
                foreach (Box parent in parents) {
                    currParents.Remove(parent);
                }
            }
        }
        CreateShape();
    }
    public void MergeShapes()
    {
        List<Shape> shapesRecieved = new List<Shape>();

        foreach (Direction d in paths.Keys)
        {
            if (recievedShapes[paths[d]] != null)
            {
                shapesRecieved.Add(recievedShapes[paths[d]]);
            }
        }

        foreach (Recipe recipe in recipes.list)
        {
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                currentShape = hold;
                return;
            }
        }

        if (shapesRecieved.Count == 0)
        {
            currentShape = null;
            currParents = new List<Box>();
        }
        else if (shapesRecieved.Count == 1)
        {
            currentShape = shapesRecieved[0];
        } else
        {
            currentShape = question;
        }
    }
}
