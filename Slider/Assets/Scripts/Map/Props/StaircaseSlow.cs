using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaircaseSlow : MonoBehaviour
{
    public Vector2 direcionalSlow = new Vector2(1, 0.7f);

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            Player.SetDirectionalMoveSpeedMultiplier(direcionalSlow);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            Player.SetDirectionalMoveSpeedMultiplier(Vector2.one);
        }
    }
}
