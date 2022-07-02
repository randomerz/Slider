using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEndOfDemoPositionChanger : MonoBehaviour
{
    // This script is so when the player finishes the demo and relaunches the save, they don't spawn 
    // in the cave transition hitbox.
    void Start()
    {
        if (Player.GetPosition().y > transform.position.y)
        {
            Player.SetPosition(Player.GetPosition() - 2 * Vector3.up);
        }
    }
    
}
