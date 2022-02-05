using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootsSpeedIncrease : MonoBehaviour
{
    // everything works, just awaiting sprite ig
    private static Player player;
    void Start()
    {
        player =  GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            player.bootsSpeedUp();
        }
    }
}
