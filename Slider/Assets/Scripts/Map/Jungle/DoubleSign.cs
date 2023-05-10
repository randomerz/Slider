using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleSign : Sign
{
    public Direction secondCurrentDirection = Direction.UP; 

    public override void CreateShape(List<Box> parents) {
        Box next = GetBoxInDirection(currentDirection);
        Box next2 = GetBoxInDirection(secondCurrentDirection);

        if (next != null)
        {
            if (currentShape != null)
            {
                if (!paths[currentDirection].isActive() || paths[currentDirection].getAnimType() == isDefaultCurrentPath(currentDirection))
                {
                    paths[currentDirection].Activate(isDefaultCurrentPath(currentDirection), currentShape); 
                    next.RecieveShape(paths[currentDirection], currentShape, parents);
                }
            }
            else
            {
                paths[currentDirection].Deactivate();
                next.RecieveShape(paths[currentDirection], currentShape, parents);
            }
        }

        if (next2 != null) {
            if (currentShape != null)
            {
                if (!paths[secondCurrentDirection].isActive() || paths[secondCurrentDirection].getAnimType() == isDefaultCurrentPath(secondCurrentDirection))
                {
                    paths[secondCurrentDirection].Activate(isDefaultCurrentPath(secondCurrentDirection), currentShape); 
                    next2.RecieveShape(paths[secondCurrentDirection], currentShape, parents);
                }
            }
            else
            {
                paths[secondCurrentDirection].Deactivate();
                next2.RecieveShape(paths[secondCurrentDirection], currentShape, parents);
            }
        }
    }

//TODO: work on rotate since it is not fully polished and doesn't really turn when stuff is in the way
    public override void Rotate(){
        if (currentShape != null)
        {
            // update the box it points in currently to push no shape onto the path
            Box box = GetBoxInDirection(currentDirection);
            Box box2 = GetBoxInDirection(secondCurrentDirection);

            if (box != null)
            {
                box.RecieveShape(paths[currentDirection], null, new List<Box>());
            }
            
            if (box2 != null)
            {
                box2.RecieveShape(paths[secondCurrentDirection], null, new List<Box>());
            }

            if (isDefaultCurrentPath(currentDirection) == paths[currentDirection].getAnimType())
            {
                paths[currentDirection].Deactivate();
            }

            if (isDefaultCurrentPath(secondCurrentDirection) == paths[secondCurrentDirection].getAnimType())
            {
                paths[secondCurrentDirection].Deactivate();
            }
            
            //check each path to see if any is not active alr
            Direction[] ds = { Direction.LEFT, Direction.UP, Direction.RIGHT, Direction.DOWN };

            // at should be less than at2
            int at = 0;
            int at2 = 0;
            int found = 0;

            for (int i = 0; i < ds.Length; i++)
            {
                if (ds[i] == currentDirection) {
                    at = i;
                    found++;
                    if (found == 2) { 
                        break;
                    }
                }
                if (ds[i] == secondCurrentDirection) {
                    at2 = i;
                    found++;
                    if (found == 2) { 
                        break;
                    }
                }
            }

            if (at > at2) {
                int temp = at;
                at = at2;
                at2 = temp;
            }

            for (int i = 1; i <= 4; i++)
            {
                Direction d = ds[(at2 + i) % 4];

                if (!paths.ContainsKey(d))
                {
                    continue;
                }

                secondCurrentDirection = d;
                //turn on path if there is not another using it
                if (!paths[d].isActive())
                {
                    Box next = GetBoxInDirection(d);
                    if (next != null)
                    {
                        if (currentShape == null)
                        {
                            return;
                        }

                        CreateShape(new List<Box>());
                    }
                    break;
                }

                //do it for the path behind
                currentDirection = ds[(at - 1) % 4];
                if (!paths[currentDirection].isActive())
                {
                    Box next = GetBoxInDirection(currentDirection);
                    if (next != null)
                    {
                        if (currentShape == null)
                        {
                            return;
                        }

                        CreateShape(new List<Box>());
                    }
                    break;
                }
            }
        }
    }
}
