using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGameObjectController : MonoBehaviour
{
    public GameObject targetGameObject;

    public void SetTargetActiveTrue()
    {
        targetGameObject.SetActive(true);
    }

    public void SetTargetActiveFalse()
    {
        targetGameObject.SetActive(false);
    }
}
