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
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateShape()
    {
        print("Box creating shape");
        //send racast in direction and then calls RecieveShape() of that obj on the boxes layer
        Physics2D.queriesHitTriggers = false;
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D raycasthit = Physics2D.Raycast(transform.position, directions[currentDirectionIndex].normalized, 100, LayerMask.GetMask("Faeries"));
        if (raycasthit.collider != null)
        {
            //do something (it shouldnt be null O_O)
            Box other = raycasthit.collider.GetComponent<Box>();
            Bin bin = raycasthit.collider.GetComponent<Bin>();

            if (other != null)
            {
                //yay we got the next box
                print("box sending shape");
                other.RecieveShape(paths[currentDirectionIndex],currentShape);
            } else if (bin != null)
            {
                print("sending shape to bin");
                bin.RecieveShape(currentShape);
            }

        } else
        {
            print("im like 100% sure u messed up somehwere :(");
        }
        Physics2D.queriesHitTriggers = true;
        Physics2D.queriesStartInColliders = false;
    }

    public virtual void RecieveShape(Path path, Shape shape)
    {
        //what should a box do when it recieves a shape... like nothing right since it just produces what its told to
        print("box recieved a shape");
        print(this.gameObject.name);
    }

    public void Rotate()
    {
        paths[currentDirectionIndex].Deactivate();
        currentDirectionIndex = (currentDirectionIndex + 1) % paths.Count;
        paths[currentDirectionIndex].Activate();
        CreateShape();
    }

    public void ChangeShape()
    {

    }
}
