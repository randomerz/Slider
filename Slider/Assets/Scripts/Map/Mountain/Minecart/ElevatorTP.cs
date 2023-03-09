using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorTP : MonoBehaviour
{
    public MinecartElevator elevator;
    public bool isTop;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(elevator.isSending) return;
        if(other.GetComponent<Minecart>()){
            Minecart mc = other.GetComponent<Minecart>();
            if(isTop)
                elevator.SendMinecartDown(mc);
            else
                elevator.SendMinecartUp(mc);
        }    
    }
}
