using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCStation : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.GetComponent<Minecart>()) {
            Debug.Log("mc2");
            other.GetComponent<Minecart>().StartMoving();
        }
    }
}
