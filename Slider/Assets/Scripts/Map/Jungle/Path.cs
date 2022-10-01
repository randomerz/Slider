using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private bool active = false;
    public Path pair;
    bool defaultAnim = true; //left, or down (animation will have default and non default for direciton
    //Animation thing

    private new void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
    }

    private new void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }


    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        ChangePair();
    }

    public void Activate(bool right)
    {
       // print("activating path: " + gameObject.name);
        active = true;
        if (right)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.green;   //right or down
            defaultAnim = true;
        } else
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.magenta; //up or left
            defaultAnim = false;
        }

        if (pair != null && !pair.isActive())
        {
            pair.Activate(right);
        }
    }

    public void Deactivate()
    {
        GetComponentInChildren<SpriteRenderer>().color = Color.white;       //unactivated
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

    public void ChangePair()
    {
        // ray cast on 2 sides
        // find short ends

        //print("changing pair for " + gameObject.name);

        pair = null;
        Vector2 one = new Vector2(1, 0);
        Vector2 two = new Vector2(-1, 0);

        if (this.transform.localEulerAngles.z == -90 || this.transform.localEulerAngles.z == 90)
        {
            one= new Vector2(0, 1);
            two = new Vector2(0, -1);
        }

        //colliders not working :<<<
        //ray cast 
        Physics2D.queriesStartInColliders = false;

        //my raycasts dont hit anything
        RaycastHit2D checkOne = Physics2D.Raycast(transform.position, one.normalized, 5, LayerMask.GetMask("JunglePaths"));
        RaycastHit2D checkTwo = Physics2D.Raycast(transform.position, two.normalized, 5, LayerMask.GetMask("JunglePaths"));


        //want to find the closest bin or box and stile
        if (checkOne.collider != null)
        {
            print("one - other path found");
            //check not on same stile
            pair = checkOne.collider.gameObject.GetComponent<Path>();
            if (!pair.transform.parent.Equals(this.transform.parent))
            {
                pair.pair = this;
            } else
            {
                pair = null;
            }
        }
        if (checkTwo.collider != null && pair == null)
        {
            print("two - other path found");
            pair = checkTwo.collider.gameObject.GetComponent<Path>();
            if (!pair.transform.parent.Equals(this.transform.parent))
            {
                pair.pair = this;
            } else
            {
                pair = null;
            }
        }

        Physics2D.queriesStartInColliders = true;
    }

    private void OnDrawGizmos()
    {
        if (this.transform.localEulerAngles.z == -90 || this.transform.localEulerAngles.z == 90)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(0, 1, 0) * 5);
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(-0, -1, 0) * 5);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(1, 0, 0) * 5);
            Gizmos.DrawLine(this.transform.position, this.transform.position + new Vector3(-1, 0, 0) * 5);
        }
    }
}
