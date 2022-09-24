using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : Box
{
    void Awake()
    {
        SetPaths();
        currentShape = shapes[currentShapeIndex];
        CreateShape();
    }

    private new void OnEnable()
    {
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private new void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CreateShape();
    }
    public void ChangeShape()
    {
        currentShapeIndex = (currentShapeIndex + 1) % shapes.Count;
        currentShape = shapes[currentShapeIndex];
        CreateShape();
    }

}
