using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCStation : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag.Equals("ButtonTrigger")) 
            other.GetComponentInParent<Minecart>().StartMoving();
    }
}
