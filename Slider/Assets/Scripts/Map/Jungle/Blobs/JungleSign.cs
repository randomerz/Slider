using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JungleSign : JungleBox
{
    [SerializeField] private RecipeList RECIPE_LIST;
    [SerializeField] private Shape JUNK_SHAPE;

    public override bool IsValidInput(JungleBox other, Direction fromDirection)
    {
        // Cannot receive in a direction I am sending
        if (direction == fromDirection)
        {
            return false;
        }

        // If the incoming and my other ids overlap, there is a loop
        
        List<int> idsExceptIncoming = new();

        foreach (Direction d in DirectionUtil.Directions)
        {
            if (d != fromDirection &&
                inputs[d] != null && 
                inputs[d].ProducedShape != null)
            {
                idsExceptIncoming.AddRange(inputs[d].UsedSourceIds);
            }
        }

        if (idsExceptIncoming.Intersect(other.UsedSourceIds).Count() != 0)
        {
            return false;
        }

        return true;
    }

    public override bool AddInput(JungleBox other, Direction fromDirection)
    {
        // Could be redundant
        bool isValidInput = IsValidInput(other, fromDirection);
        
        if (isValidInput)
        {
            inputs[fromDirection] = other;
        }

        return isValidInput;
    }

    public override void RemoveInput(Direction fromDirection)
    {
        inputs[fromDirection] = null;
        // isUsingInput[fromDirection] = false;

        if (direction == fromDirection && targetBox != null)
        {
            // The other box should already have this as an input, but is not using it
            targetBox.AddInput(this, DirectionUtil.Inv(fromDirection));
            targetBox.UpdateBox();
        }
    }

    public override void UpdateBox(int depth = 0)
    {
        if (depth >= DEPTH_LIMIT)
        {
            Debug.LogError("Jungle Box depth limit exceeded!");
            return;
        }

        // Collect shapes and ids
        List<Shape> shapes = new();
        List<int> ids = new();

        foreach (Direction d in DirectionUtil.Directions)
        {
            if (inputs[d] != null && inputs[d].ProducedShape != null)
            {
                shapes.Add(inputs[d].ProducedShape);
                ids.AddRange(inputs[d].UsedSourceIds);
            }
        }

        // Merge shapes
        Shape oldShape = ProducedShape;
        Shape newShape = CraftShape(shapes);

        // Set my shape
        ProducedShape = newShape;
        UsedSourceIds = ids;

        if (newShape == oldShape)
        {
            // Nothing changed! Don't keep propogating
            return;
        }

        // Update sprites
        UpdateSprites();

        // Update next box
        // if (ProducedShape != null && targetBox != null)
        Direction invDirection = DirectionUtil.Inv(direction);
        if (targetBox != null)
        {
            if (targetBox.IsValidInput(this, invDirection))
            {
                // For cases where boxes are in a loop and one of the loop
                // inputs gets removed, so the loop needs to re-propogate.
                targetBox.AddInput(this, invDirection);

                targetBox.UpdateBox(depth + 1);
            }
            else
            {
                // Same loop case as above
                targetBox.RemoveInput(invDirection);
            }
        }
    }

    private Shape CraftShape(List<Shape> shapes)
    {
        if (shapes == null || shapes.Count == 0)
        {
            return null;
        }

        if (shapes.Count == 1)
        {
            return shapes[0];
        }

        foreach (Recipe recipe in RECIPE_LIST.list)
        {
            Shape s = recipe.Check(shapes);
            if (s != null)
            {
                return s;
            }
        }

        return JUNK_SHAPE;
    }
}