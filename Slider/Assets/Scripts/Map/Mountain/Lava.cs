using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>()){
            other.gameObject.GetComponent<Meltable>().AddLava(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>())
            other.gameObject.GetComponent<Meltable>().RemoveLava(this);
    }
}
