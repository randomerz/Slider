using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Path : MonoBehaviour
{
    private bool active = false;
    private bool creatingBlobs = true;
    public Path pair;
    private Shape currentShape = null;
    bool defaultAnim = true; //right, or down (animation will have default and non default for direciton
    public bool horizontal = false;


    [Header("Animation Blobs")]
    public GameObject blob;
    private Direction direction;
    private float timeBetweenCreation = 3.8f;
    private float travelDistance = 0;

    private float timeCount = 4;

    void Start()
    {
    }

    void Update()
    {
        if (active && creatingBlobs)
        {
            if (timeCount >= timeBetweenCreation)
            {
                CreateBlob();
                timeCount = 0;
            }
            timeCount += Time.deltaTime;
        }
    }

    public void CreateBlob()
    {
        GameObject go = Instantiate(blob);
        Blob new_blob = go.GetComponent<Blob>();
        new_blob.transform.parent = this.transform;

        BoxCollider2D collider = this.GetComponent<BoxCollider2D>();
        travelDistance = (int)this.transform.localScale.x + 0.5f;
        if (pair != null)
        {
            travelDistance += (int)pair.transform.localScale.x;
        }

        new_blob.UpdateBlobOnPath(defaultAnim, direction, travelDistance, pair, currentShape);

        // set blob to be the correct starting position
        if (direction == Direction.LEFT || direction == Direction.DOWN)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2), 0, 0);
        }
        else
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x - (collider.size.x / 2), 0, 0);
        }
       
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
        {
            new_blob.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -1;
        }
    }

    public void Activate(bool right, Shape shape, bool creating = true)
    {
        //print("activating path: " + right + " for " + this.gameObject.name);
        creatingBlobs = creating;
        active = true;

        if (right)
        {
            //GetComponentInChildren<SpriteRenderer>().color = Color.green;   //right or down
            defaultAnim = true;
        } else
        {
            //GetComponentInChildren<SpriteRenderer>().color = Color.magenta; //up or left
            defaultAnim = false;
        }

        if (horizontal)
        {
            if (defaultAnim)
            {
                direction = Direction.RIGHT;
            }
            else
            {
                direction = Direction.LEFT;
            }
        }
        else
        {
            if (defaultAnim)
            {
                direction = Direction.DOWN;
            }
            else
            {
                direction = Direction.UP;
            }
        }

        if (pair != null && !pair.isActive())
        {
            pair.Activate(right, shape, false);
        }

        currentShape = shape;
    }

    public void Deactivate()
    {
        currentShape = null;
       // GetComponentInChildren<SpriteRenderer>().color = Color.white;       //unactivated
        active = false;

        if (pair != null && pair.isActive())
        {
            pair.Deactivate();
        }
    }

    public bool isActive()
    {
        return active;
    }

    public bool getAnimType()
    {
        return defaultAnim;
    }

    public void ChangePair()
    {
        pair = null;
        Vector2 one = new Vector2(1, 0);
        Vector2 two = new Vector2(-1, 0);


        if (!horizontal)
        {
            one = new Vector2(0, 1);
            two = new Vector2(0, -1);
        }

        Physics2D.queriesStartInColliders = false;

        RaycastHit2D checkOne = Physics2D.Raycast(transform.position, one.normalized, 7, LayerMask.GetMask("JunglePaths"));
        RaycastHit2D checkTwo = Physics2D.Raycast(transform.position, two.normalized, 7, LayerMask.GetMask("JunglePaths"));

        // print("");
        //want to find the closest bin or box and stile
        if (checkOne.collider != null)
        {
            //check not on same stile
            pair = checkOne.collider.gameObject.GetComponent<Path>();
            if (!pair.transform.parent.Equals(this.transform.parent))
            {
                pair.pair = this;
            }
            else
            {
                pair = null;
            }
        }
        if (checkTwo.collider != null && pair == null)
        {
            pair = checkTwo.collider.gameObject.GetComponent<Path>();
            if (!pair.transform.parent.Equals(this.transform.parent))
            {
                pair.pair = this;
            }
            else
            {
                pair = null; 
            }
        }

        Physics2D.queriesStartInColliders = true;
    }

/*    private void OnDrawGizmos()
    {
        if (this.transform.localEulerAngles.z == -90 || this.transform.localEulerAngles.z == 90)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(0, 1, 0) * 5);
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(0, -1, 0) * 5);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(1, 0, 0) * 5);
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(-1, 0, 0) * 5);
        }
    }*/
}
