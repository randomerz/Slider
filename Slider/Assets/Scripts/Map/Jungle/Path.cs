using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private bool active = false;
    public Path pair;
    bool defaultAnim = true; //left, or down (animation will have default and non default for direciton
    private Vector2 direction;
    //Animation thing
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void Activate(bool right)
    {
        print("activating path: " + gameObject.name);
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

}
