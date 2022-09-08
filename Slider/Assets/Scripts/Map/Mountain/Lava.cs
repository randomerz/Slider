using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Lava : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>()){
            other.gameObject.GetComponent<Meltable>().AddLava();
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>())
            other.gameObject.GetComponent<Meltable>().RemoveLava();
    }
}
