using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCStation : MonoBehaviour
{
    public int direction = 2;

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag.Equals("ButtonTrigger")) 
        {
            Minecart mc =  other.GetComponentInParent<Minecart>();
            if(mc == null) return;
            if(!mc.isMoving) 
                mc.StartMoving();
            else if(mc.currentDirection != direction)
                mc.StopMoving();
                //chaneg direction
                //recalculate target
                //go go go
        }
    }
}
