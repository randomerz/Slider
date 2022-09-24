using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public bool active = false;
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
    }

    public void Deactivate()
    {
        GetComponentInChildren<SpriteRenderer>().color = Color.white;       //unactivated
        active = false;
    }

    public bool isActive()
    {
        return active;
    }

}
