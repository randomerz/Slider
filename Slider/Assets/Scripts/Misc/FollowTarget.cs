using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public GameObject target;
    void LateUpdate()
    {
        transform.position = target.transform.position;
    }
}
