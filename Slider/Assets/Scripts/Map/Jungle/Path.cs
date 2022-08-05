using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    bool active = false;
    bool defaultAnim = true; //left, or down (animation will have default and non default for direciton
    private Vector2 direction;
    //Animation thing
    // Start is called before the first frame update
    void Start()
    {
        direction = new Vector2 (Mathf.Cos(transform.rotation.eulerAngles.z),Mathf.Sin(transform.rotation.eulerAngles.z));
    }

    public Vector2 getDirection()
    {
        return direction;
    }

    public void Activate()
    {

    }

    public void Deactivate()
    {

    }
}
