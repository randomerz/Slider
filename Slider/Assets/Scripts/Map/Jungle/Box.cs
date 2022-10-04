using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    protected Dictionary<string, int> stringToIndex = new Dictionary<string, int>();

    public List<Shape> shapes;
    protected int currentShapeIndex = 0;
    protected Shape currentShape;

    public Path left;
    public Path right;
    public Path top;
    public Path bottom;

    protected List<Path> paths = new List<Path>(); //vector2 key, path
    protected List<Vector2> directions = new List<Vector2>();
    public int currentDirectionIndex = 0;

    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();
    }

    private new void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
    }

    private new void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
    }

    protected void DeactivatePathsOnSTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Path path in paths)
        {
            path.Deactivate();
        }
    }

    protected void SetPaths()
    {
        if (left != null)
        {
            directions.Add(new Vector2(-1, 0));
            paths.Add(left);
            stringToIndex.Add("left", stringToIndex.Count);
        }
        if (top != null)
        {
            directions.Add(new Vector2(0, 1));
            paths.Add(top);
            stringToIndex.Add("top", stringToIndex.Count);
        }
        if (right != null)
        {
            directions.Add(new Vector2(1, 0));
            paths.Add(right);
            stringToIndex.Add("right", stringToIndex.Count);
        }
        if (bottom != null)
        {
            directions.Add(new Vector2(0, -1));
            paths.Add(bottom);
            stringToIndex.Add("down", stringToIndex.Count);
        }
    }

    public void CreateShape()
    {
        //print("Box creating shape");

        /*        Physics2D.queriesStartInColliders = false;

                RaycastHit2D[] tileCheck = Physics2D.RaycastAll(transform.position, directions[currentDirectionIndex].normalized, 100, LayerMask.GetMask("Slider"));

                Physics2D.queriesStartInColliders = true;

                Box nextBox = null;
                Bin nextBin = null;
                float distanceTo = 100;
                float inactiveStileDistance = 100;

                //want to find the closest bin or box and stile
                foreach (RaycastHit2D raycasthit in tileCheck)
                {
                    Collider2D hitcollider = raycasthit.collider;
                    if (raycasthit.collider != null)
                    {
                        //do something (it shouldnt be null O_O)
                        STile s = hitcollider.gameObject.GetComponent<STile>();
                        Box other = hitcollider.GetComponent<Box>();
                        Bin bin = hitcollider.GetComponent<Bin>();

                        if (s != null && !s.isTileActive)
                        {
                            if (Vector2.Distance(raycasthit.centroid, transform.position) < inactiveStileDistance)
                            {
                                inactiveStileDistance = Vector2.Distance(raycasthit.centroid, transform.position);
                            }
                        }

                        if (other != null || bin != null)
                        {
                            if (Vector2.Distance(raycasthit.centroid, transform.position) < distanceTo)
                            {
                                distanceTo = Vector2.Distance(raycasthit.centroid, transform.position);

                                //save the closest box/bin
                                if (other != null)
                                {
                                    nextBox = other;
                                }
                                else
                                {
                                    nextBin = bin;
                                }
                            }
                        }
                    }
                }

                if (distanceTo > inactiveStileDistance)
                {
                    //print("inactive stile in the way");
                    nextBin = null;
                    nextBox = null;
                }

                //now see if there is an avaliable bin or box
                if (nextBox != null)
                {
                    //yay we got the next box
                   //print("box sending shape");
                    nextBox.RecieveShape(paths[currentDirectionIndex], currentShape);

                    //only show path working if there is a shape to carry and if not the path is deactivated did i wanna change this? iforgot
                    if (currentShape != null)
                    {
                        paths[currentDirectionIndex].Activate(isDefaultCurrentPath());
                    } else
                    {
                        paths[currentDirectionIndex].Deactivate();
                    }
                }
                else if (nextBin != null)
                {
                   // print("sending shape to bin");
                    nextBin.RecieveShape(currentShape);

                    if (currentShape != null)
                    {
                        paths[currentDirectionIndex].Activate(isDefaultCurrentPath());
                    } else
                    {
                        paths[currentDirectionIndex].Deactivate();
                    }
                }*/
        Box next = GetBoxInDirection();
        if (next != null)
        {
            next.RecieveShape(paths[currentDirectionIndex], currentShape);
            if (currentShape != null)
            {
                paths[currentDirectionIndex].Activate(isDefaultCurrentPath());
            }
            else
            {
                paths[currentDirectionIndex].Deactivate();
            }
        }
    }


    public virtual void RecieveShape(Path path, Shape shape)
    {
        //what should a box do when it recieves a shape... like nothing right since it just produces what its told to
        // print("box recieved a shape");
        //  print(this.gameObject.name);
    }

    public void Rotate()
    {
        if (currentShape != null)
        {
            paths[currentDirectionIndex].Deactivate();

            //check each path to see if any is not active alr
            for (int i = 0; i < paths.Count; i++)
            {
                currentDirectionIndex = (currentDirectionIndex + 1) % paths.Count;

                //start path if that path is not active alr
                if (!paths[currentDirectionIndex].isActive())
                {
                    Box next = GetBoxInDirection();
                    if (next != null)
                    {
                        if (currentShape == null)
                        {
                            return;
                        }
                        paths[currentDirectionIndex].Activate(isDefaultCurrentPath());

                        CreateShape();
                    }
                    break;
                }
            }
        }
    }

    private Box GetBoxInDirection()
    {
        Physics2D.queriesStartInColliders = false;

        RaycastHit2D[] tileCheck = Physics2D.RaycastAll(transform.position, directions[currentDirectionIndex].normalized, 100, LayerMask.GetMask("Slider"));

        Physics2D.queriesStartInColliders = true;

        Box nextBox = null;
        float distanceTo = 100;
        float inactiveStileDistance = 100;

        //want to find the closest bin or box and stile
        foreach (RaycastHit2D raycasthit in tileCheck)
        {
            Collider2D hitcollider = raycasthit.collider;
            if (raycasthit.collider != null)
            {
                //do something (it shouldnt be null O_O)
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
            //print("inactive stile in the way");
            nextBox = null;
        }

        return nextBox;

    }

    protected bool isDefaultCurrentPath()
    {
        bool defaultPath = false;
        int right = -1;
        int down = -1;
        if (stringToIndex.ContainsKey("right"))
        {
            right = stringToIndex["right"];
        }
        if (stringToIndex.ContainsKey("down"))
        {
            down = stringToIndex["down"];
        }

        if (currentDirectionIndex == right || currentDirectionIndex == down) //down or right 
        {
            defaultPath = true;
        }
        return defaultPath;
    }

    public Vector2 GetDirection()
    {
        return directions[currentDirectionIndex];
    }
}
