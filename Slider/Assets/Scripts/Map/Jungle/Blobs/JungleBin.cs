using UnityEngine;
using System.Collections.Generic;
using System;

public class JungleBin : JungleBox
{
    public static System.EventHandler<JungleBinArgs> OnBinRecieveShape;

    public class JungleBinArgs : System.EventArgs
    {
        public Shape shape;
    }

    [SerializeField] private List<ShapePlacer> shapePlacers;

    protected override void Awake()
    {
        base.Awake();

        direction = Direction.UP;
    }

    public override bool IsValidInput(JungleBox other, Direction fromDirection)
    {
        return true;
    }

    public override bool AddInput(JungleBox other, Direction fromDirection)
    {
        inputs[fromDirection] = other;

        return IsValidInput(other, fromDirection);
    }

    public override void RemoveInput(Direction fromDirection)
    {
        inputs[fromDirection] = null;
    }

    public override bool UpdateBox(int depth = 0)
    {
        if (depth >= DEPTH_LIMIT)
        {
            Debug.LogError("Jungle Box depth limit exceeded!");
            return false;
        }

        foreach (ShapePlacer sp in shapePlacers)
        {
            sp.Stop();
        }

        // Collect shapes and ids
        List<Shape> shapes = new();

        foreach (Direction d in DirectionUtil.Directions)
        {
            if (inputs[d] != null && inputs[d].ProducedShape != null)
            {
                shapes.Add(inputs[d].ProducedShape);
            }
        }
        ProducedShape = shapes.Count > 0 ? shapes[0] : null; // for debug

        for (int i = 0; i < Math.Min(shapes.Count, shapePlacers.Count); i++)
        {
            Shape shape = shapes[i];

            JungleRecipeBookSave.IncrementNumberCreated(shape);
            OnBinRecieveShape?.Invoke(this, new JungleBinArgs { shape = shape });
            
            shapePlacers[i].gameObject.SetActive(true);
            shapePlacers[i].Place(shape);
        }

        UpdateSprites();

        return false;
    }

    protected override void UpdateSprites()
    {
        base.UpdateSprites();
    }
}