using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JungleSign : JungleBox
{
    [SerializeField] private RecipeList RECIPE_LIST;
    [SerializeField] private Shape JUNK_SHAPE;

    public override bool IsValidInput(JungleBox other, Direction fromDirection)
    {
        return IsValidInDirection(other, fromDirection, direction);
    }

    protected bool IsValidInDirection(JungleBox other, Direction fromDirection, Direction myDirection)
    {
        // Cannot receive in a direction I am sending
        if (myDirection == fromDirection)
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
                List<int> sourceIds = inputs[d].GetUsedSourceIds(DirectionUtil.Inv(d));
                idsExceptIncoming.AddRange(sourceIds);
            }
        }

        List<int> otherSourceIds = other.GetUsedSourceIds(DirectionUtil.Inv(fromDirection));
        if (idsExceptIncoming.Intersect(otherSourceIds).Count() != 0)
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

        TryRestoreOnRemove(fromDirection, direction, targetBox);
    }

    protected void TryRestoreOnRemove(Direction fromDirection, Direction myDirection, JungleBox myTargetBox)
    {
        if (myDirection == fromDirection && myTargetBox != null)
        {
            // The other box should already have this as an input, but is not using it
            myTargetBox.AddInput(this, DirectionUtil.Inv(fromDirection));
            myTargetBox.UpdateBox();
        }

    }

    public override bool UpdateBox(int depth = 0)
    {
        if (depth >= DEPTH_LIMIT)
        {
            Debug.LogError("Jungle Box depth limit exceeded!");
            return false;
        }

        // Collect shapes and ids
        List<Shape> shapes = new();
        List<int> ids = new();

        foreach (Direction d in DirectionUtil.Directions)
        {
            if (inputs[d] != null && inputs[d].ProducedShape != null)
            {
                shapes.Add(inputs[d].ProducedShape);
                ids.AddRange(inputs[d].GetUsedSourceIds(DirectionUtil.Inv(d)));
            }
        }

        // Merge shapes
        Shape oldShape = ProducedShape;
        Shape newShape = CraftShape(shapes);

        // Set my shape
        ProducedShape = newShape;
        usedSourceIds = ids;

        if (newShape == oldShape)
        {
            // Nothing changed! Don't keep propogating
            return false;
        }

        JungleRecipeBookSave.IncrementNumberCreated(ProducedShape);

        // Update sprites
        UpdateSprites();
        pathController.UpdateMarchingShape(direction, ProducedShape);

        UpdatePropogateInDirection(direction, targetBox, signAnimator, depth);
        return true;
    }

    protected void UpdatePropogateInDirection(Direction myDirection, JungleBox myTargetBox, JungleSignAnimator mySignAnimator, int depth)
    {
        // Update next box
        // if (ProducedShape != null && myTargetBox != null)
        Direction invDirection = DirectionUtil.Inv(myDirection);
        if (myTargetBox != null)
        {
            if (myTargetBox.IsValidInput(this, invDirection))
            {
                // For cases where boxes are in a loop and one of the loop
                // inputs gets removed, so the loop needs to re-propogate.
                myTargetBox.AddInput(this, invDirection);
                myTargetBox.UpdateBox(depth + 1);

                SetIsSending(ProducedShape != null, myDirection, myTargetBox, mySignAnimator);
            }
            else
            {
                // Same loop case as above
                myTargetBox.RemoveInput(invDirection);

                // SetIsSending(false, myDirection, myTargetBox);
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