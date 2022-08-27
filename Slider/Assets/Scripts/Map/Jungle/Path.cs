using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    bool active = false;
    bool defaultAnim = true; //left, or down (animation will have default and non default for direciton
    private Vector2 direction;
    bool oneSideOn = false;
    bool twoSideOn = false;
    //Animation thing
    // Start is called before the first frame update
    void Start()
    {
        direction = new Vector2 (Mathf.Cos(transform.rotation.eulerAngles.z),Mathf.Sin(transform.rotation.eulerAngles.z));
    }

    public void Activate()
    {
        GetComponentInChildren<SpriteRenderer>().color = Color.green;
    }

    public void Deactivate()
    {
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }

    public void TurnSideOn()
    {
        if (oneSideOn)
        {
            twoSideOn = true;
        } else
        {
            oneSideOn = true;
            Activate();
        }
    }

    public void TurnSideOff()
    {
        if (twoSideOn)
        {
            twoSideOn = false;
        } 
        else if (oneSideOn)
        {
            oneSideOn = false;
            Deactivate();
        }
    }
}
