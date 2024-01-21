using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleSign : Sign
{
    public Direction secondCurrentDirection = Direction.UP; 

    public override void CreateShape() {
/*        if (currentShape == null)
        {
            print("Double sign sending null");
        }
        else
        {
            print("Double sign sending shape " + currentShape.name);
        }*/

        Box next = GetBoxInDirection(currentDirection);
        Box next2 = GetBoxInDirection(secondCurrentDirection);

        if (!currParents.Contains(this)){
            currParents.Add(this);
        }

        if (next != null)
        {
            if (currentShape != null)
            {
                if (!paths[currentDirection].isActive() || paths[currentDirection].getAnimType() == isDefaultCurrentPath(currentDirection))
                {
                    paths[currentDirection].Activate(isDefaultCurrentPath(currentDirection), currentShape);
                    List<Box> parents = GetParents(); 
                    next.RecieveShape(paths[currentDirection], currentShape, parents);
                }
            }
            else
            {
                paths[currentDirection].Deactivate();
                List<Box> parents = GetParents();
                next.RecieveShape(paths[currentDirection], currentShape, parents);
            }
        }

        if (next2 != null) {
            if (currentShape != null)
            {
                if (!paths[secondCurrentDirection].isActive() || paths[secondCurrentDirection].getAnimType() == isDefaultCurrentPath(secondCurrentDirection))
                {
                    paths[secondCurrentDirection].Activate(isDefaultCurrentPath(secondCurrentDirection), currentShape); 
                    List<Box> parents = GetParents();
                    next2.RecieveShape(paths[secondCurrentDirection], currentShape, parents);
                }
            }
            else
            {
                paths[secondCurrentDirection].Deactivate();
                List<Box> parents = GetParents();
                next2.RecieveShape(paths[secondCurrentDirection], currentShape, parents);
            }
        }
    }

    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        if (parents.Contains(this))
        {
            path.Deactivate();
            return;
        }

        if (shape != null) {
            currParents = parents;
        }

        //check if it is the input path
        if (path.pair != paths[currentDirection] && path.pair != paths[secondCurrentDirection]) {
            if (path.pair != null)
            {
                recievedShapes[path.pair] = shape;
                MergeShapes();
                CreateShape();
            }
            else
            {
                recievedShapes[path] = shape;
                this.MergeShapes();
                this.CreateShape();
            }
        } else {
            path.Deactivate();
        }
    }

    public override void Rotate()
    {
        if (currentShape != null) {
            return;
        }

        // update the box it points in currently to push no shape onto the path
        Box box = GetBoxInDirection(currentDirection);
        Box box2 = GetBoxInDirection(secondCurrentDirection);

        if (box != null && isDefaultCurrentPath(currentDirection) == paths[currentDirection].getAnimType())
        {
            List<Box> parents = new List<Box>();
            parents.Add(this);

            box.RecieveShape(paths[currentDirection], null, parents);
            paths[currentDirection].Deactivate();
        }
        
        if (box2 != null && isDefaultCurrentPath(secondCurrentDirection) == paths[secondCurrentDirection].getAnimType())
        {

            List<Box> parents = new List<Box>();
            parents.Add(this);

            box2.RecieveShape(paths[secondCurrentDirection], null, parents);
            paths[secondCurrentDirection].Deactivate();
        }
        
        //check each path to see if any is not active alr
        Direction[] ds = { Direction.LEFT, Direction.UP, Direction.RIGHT };

        Direction hold1 = currentDirection;
        Direction hold2 = secondCurrentDirection;

        for (int i = 0; i < ds.Length; i++)
        {
            if (ds[i] == hold1) {
                continue;
            }
            else if (ds[i] == hold2) {
                currentDirection = hold2;
            } else {
                secondCurrentDirection = ds[i];
                paths[ds[i]].Deactivate();
            }
        }

        CreateShape();
    }
}
