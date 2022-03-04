using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITracker : MonoBehaviour
{

    public GameObject target;
    public Image image;
    private float minY = -75;

    public STile GetSTile(){
        return SGrid.current.GetStileUnderneath(target);
    }

    public Vector2 getPosition(){
        return target.transform.position;
    }

    public bool GetIsInHouse(){
        if(target.transform.position.y < minY)
            return true;
        return false;
    }
}
