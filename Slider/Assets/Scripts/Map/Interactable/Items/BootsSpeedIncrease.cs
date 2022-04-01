using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootsSpeedIncrease : MonoBehaviour
{
    // everything works, just awaiting sprite ig
    private static Player player;
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        
    }
    
    public void SpeedUp()
    {
        player.UpdatePlayerSpeed();
    }
}
