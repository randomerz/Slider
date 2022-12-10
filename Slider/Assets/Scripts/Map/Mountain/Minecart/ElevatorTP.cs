using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorTP : MonoBehaviour
{
    public MinecartElevator elevator;
    public bool isTop;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.GetComponent<Minecart>()){
            Debug.Log("Elevator TP");
            Minecart mc = other.GetComponent<Minecart>();
            if(isTop)
                elevator.SendMinecartDown(mc);
            else
                elevator.SendMinecartUp(mc);
        }    
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
