using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCStation : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag.Equals("ButtonTrigger")) 
            other.GetComponentInParent<Minecart>().StartMoving();
        
        if(other.GetComponent<Minecart>() != null) {
            Minecart mc = other.GetComponent<Minecart>();
            if(mc.isMoving && mc.isOnTrack)
            {
                mc.Derail();
            }
        }
    }
}
