using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : Box
{
    void Awake()
    {
        SetPaths();

        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }

        currentShape = shapes[currentShapeIndex];
        CreateShape(new List<Box>());
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += OnSTileEnabled;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CreateShape(new List<Box>());
    }
    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }
        CreateShape(new List<Box>());
    }

    public void ChangeShape()
    {
        Box box = GetBoxInDirection();

        if (box != null)
        {
            box.RecieveShape(paths[currentDirection], null, new List<Box>());
        }
        paths[currentDirection].Deactivate();

        currentShapeIndex = (currentShapeIndex + 1) % shapes.Count;
        currentShape = shapes[currentShapeIndex];
        CreateShape(new List<Box>());
    }
}
