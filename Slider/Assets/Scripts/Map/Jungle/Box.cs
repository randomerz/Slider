using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public Path left;
    public Path right;
    public Path top;
    public Path bottom;

    protected List<Path> paths = new List<Path>(); //vector2 key, path
    protected List<Vector2> directions = new List<Vector2>();
    protected int currentDirectionIndex = 0;
    public List<Shape> shapes;
    protected Shape currentShape;

    // Start is called before the first frame update
    void Awake()
    {
        //start a path here in a default direction
        if (left != null)
        {
            directions.Add(new Vector2(-1, 0));
            paths.Add(left);
        }
        if (right != null)
        {
            directions.Add(new Vector2(1, 0));
            paths.Add(right);
        }
        if (top != null)
        {
            directions.Add(new Vector2(0, 1));
            paths.Add(top);
        }
        if (bottom != null)
        {
            directions.Add(new Vector2(0, -1));
            paths.Add(bottom);
        }

        currentShape = shapes[0];
        CreateShape();

        //initiate shapes here or above
    }

    private new void OnEnable()
    {
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private new void OnDisable()
    {
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //will need this
/*    private void CreateShape(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        CreateShape();
    }*/

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs e)
    {
        CreateShape();
    }


    public void CreateShape()
    {
       // print("Box creating shape");
        //send racast in direction and then calls RecieveShape() of that obj on the boxes layer

        Physics2D.queriesStartInColliders = false;

        RaycastHit2D[] tileCheck = Physics2D.RaycastAll(transform.position, directions[currentDirectionIndex].normalized, 100, LayerMask.GetMask("Slider"));

        Physics2D.queriesStartInColliders = true;

        //       RaycastHit2D raycasthit = Physics2D.Raycast(transform.position, directions[currentDirectionIndex].normalized, 100, LayerMask.GetMask("Faeries"));

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
            print("inactive stile in the way");
            nextBin = null;
            nextBox = null;
        }

        //now see if there is an avaliable bin or box
        if (nextBox != null)
        {
            //yay we got the next box
           // print("box sending shape");
            nextBox.RecieveShape(paths[currentDirectionIndex], currentShape);
            paths[currentDirectionIndex].Activate();
        }
        else if (nextBin != null)
        {
           // print("sending shape to bin");
            nextBin.RecieveShape(currentShape);
        }
    }


    public virtual void RecieveShape(Path path, Shape shape)
    {
        //what should a box do when it recieves a shape... like nothing right since it just produces what its told to
        print("box recieved a shape");
        print(this.gameObject.name);
    }

    //TODO: make this work now! and add path color changes so IK what path is being used!
    //Then check shape combinations!
    public void Rotate()
    {
        //paths[currentDirectionIndex].Deactivate();
       //currentDirectionIndex = (currentDirectionIndex + 1) % paths.Count;
       // paths[currentDirectionIndex].Activate();
        CreateShape();
    }

    public void ChangeShape()
    {

    }
}
