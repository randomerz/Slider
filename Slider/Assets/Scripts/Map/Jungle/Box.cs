using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    protected Dictionary<Direction, Path> paths = new Dictionary<Direction, Path>();

    public List<Shape> shapes;
    protected int currentShapeIndex = 0;

    public List<Box> parents = new List<Box>();
    public Shape currentShape;

    public Path left;
    public Path right;
    public Path top;
    public Path bottom;

    protected List<Vector2> directions = new List<Vector2>();
    public Direction currentDirection = Direction.RIGHT; //you should set at the start 

    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();
        parents.Add(this);
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
        SGrid.OnSTileEnabled += STileEnabled;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
        SGrid.OnSTileEnabled -= STileEnabled;
    }

    protected void DeactivatePathsOnSTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Direction d in paths.Keys)
        {
            paths[d].Deactivate();
        }
    }

    protected void STileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CreateShape();
    }

    protected void SetPaths()
    {
        if (left != null)
        {
            paths[Direction.LEFT] = left;
        }
        if (top != null)
        {
            paths[Direction.UP] = top;
        }
        if (right != null)
        {
            paths[Direction.RIGHT] = right;
        }
        if (bottom != null)
        {
            paths[Direction.DOWN] = bottom;
        }
    }

    public void CreateShape()
    {
       // print(this.gameObject.name + " is sending shape " + currentShape);
        //print(currentDirection);
        Box next = GetBoxInDirection();
        if (next != null)
        {
            if (currentShape != null)
            {
                if (!paths[currentDirection].isActive() || paths[currentDirection].getAnimType() == isDefaultCurrentPath())//something wrong here
                {
                    paths[currentDirection].Activate(isDefaultCurrentPath(), currentShape); 
                    next.RecieveShape(paths[currentDirection], currentShape, parents);
                }
            }
            else
            {
                paths[currentDirection].Deactivate();
            }
        }
    }


    public virtual void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        
    }

    public void Rotate()
    {
        if (currentShape != null)
        {
            // update the box it points in currently to push no shape onto the path
            Box box = GetBoxInDirection();

            if (box != null)
            {
                box.RecieveShape(paths[currentDirection], null, this.parents);
            }

            if (isDefaultCurrentPath() == paths[currentDirection].getAnimType())
            {
                paths[currentDirection].Deactivate();
            }

            //check each path to see if any is not active alr

            Direction[] ds = { Direction.LEFT, Direction.UP, Direction.RIGHT, Direction.DOWN };

            int at = 0;

            for (int i = 0; i < ds.Length; i++)
            {
                if (ds[i] == currentDirection) {
                    at = i;
                    break;
                }
            }

            for (int i = 1; i <= 4; i++)
            {
                Direction d = ds[(at + i) % 4];
                //print(d);

                if (!paths.ContainsKey(d))
                {
                    continue;
                }

                currentDirection = d;
                //turn on path if there is not another using it
                if (!paths[d].isActive())
                {
                    Box next = GetBoxInDirection();
                    if (next != null)
                    {
                        if (currentShape == null)
                        {
                            return;
                        }

                        CreateShape();
                    }
                    break;
                }
            }
        }
    }

    private Box GetBoxInDirection()
    {
        Vector2 v = DirectionUtil.D2V(currentDirection);

        Physics2D.queriesStartInColliders = false;
        Physics2D.queriesHitTriggers = false;

        RaycastHit2D[] tileCheck = Physics2D.RaycastAll(transform.position, v.normalized, 100, LayerMask.GetMask("Slider"));

        Box nextBox = null;
        float distanceTo = 100;
        float inactiveStileDistance = 100;

        //want to find the closest bin or box and stile
        foreach (RaycastHit2D raycasthit in tileCheck)
        {
            Collider2D hitcollider = raycasthit.collider;
            if (raycasthit.collider != null)
            {
                STile s = hitcollider.gameObject.GetComponent<STile>();
                Box other = hitcollider.GetComponent<Box>();

                if (s != null && !s.isTileActive)
                {
                    if (Vector2.Distance(raycasthit.centroid, transform.position) < inactiveStileDistance)
                    {
                        inactiveStileDistance = Vector2.Distance(raycasthit.centroid, transform.position);
                    }
                }

                if (other != null)
                {
                    if (Vector2.Distance(raycasthit.centroid, transform.position) < distanceTo)
                    {
                        distanceTo = Vector2.Distance(raycasthit.centroid, transform.position);
                        nextBox = other;
                    }
                }
            }
        }

        if (distanceTo > inactiveStileDistance)
        {
            nextBox = null;
        }

        Physics2D.queriesHitTriggers = true;
        Physics2D.queriesStartInColliders = true;

        return nextBox;

    }

    protected bool isDefaultCurrentPath()
    {
        return currentDirection == Direction.RIGHT || currentDirection == Direction.DOWN;
    }

    public Vector2 GetDirection()
    {
       return DirectionUtil.D2V(currentDirection);
    }
}
