using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Path : MonoBehaviour
{
    public bool active = false;
    public Path pair;
    bool defaultAnim = true; //left, or down (animation will have default and non default for direciton
    Direction direction;

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
        pair = null;
        Vector2 one = new Vector2(1, 0);
        Vector2 two = new Vector2(-1, 0);


        if (this.transform.localEulerAngles.z == -90 || this.transform.localEulerAngles.z == 90)
        {
            one = new Vector2(0, 1);
            two = new Vector2(0, -1);
        }

        Physics2D.queriesStartInColliders = false;

        RaycastHit2D checkOne = Physics2D.Raycast(transform.position, one.normalized, 6, LayerMask.GetMask("JunglePaths"));
        RaycastHit2D checkTwo = Physics2D.Raycast(transform.position, two.normalized, 6, LayerMask.GetMask("JunglePaths"));

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

    private void OnDrawGizmos()
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
    }
}
