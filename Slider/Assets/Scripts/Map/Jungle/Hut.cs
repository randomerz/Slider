using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : Box
{
    public Sprite[] insignias;
    public SpriteRenderer insignia;

    void Awake()
    {
        SetPaths();

        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }

        currentShape = shapes[currentShapeIndex];
        CreateShape(new List<string>());
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
        CreateShape(new List<string>());
    }
    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }
        CreateShape(new List<string>());
    }

    public void ChangeShape()
    {
        Box box = GetBoxInDirection(currentDirection);

        if (box != null)
        {
            box.RecieveShape(paths[currentDirection], null, new List<string>());
        }
        paths[currentDirection].Deactivate();

        currentShapeIndex = (currentShapeIndex + 1) % shapes.Count;
        currentShape = shapes[currentShapeIndex];
        insignia.sprite = insignias[currentShapeIndex];
        CreateShape(new List<string>());
    }
    
    public void IsShapeTriangle(Condition c) => c.SetSpec(currentShape.shapeName == "Triangle");
    public void IsShapeSemiCircle(Condition c) => c.SetSpec(currentShape.shapeName == "Semicircle");
    public void IsShapeLine(Condition c) => c.SetSpec(currentShape.shapeName == "Line");
}
