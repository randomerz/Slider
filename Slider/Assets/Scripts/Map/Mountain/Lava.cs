using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Lava : MonoBehaviour
{
   /* private void OnEnable() {
        MountainSGridAnimator.OnSTileMoveEndEarly += EnableOnEnd;
        MountainSGridAnimator.OnSTileMoveStart += DisableOnStart;
    }

    private void OnDisable() {
        MountainSGridAnimator.OnSTileMoveEndEarly -= EnableOnEnd;
        MountainSGridAnimator.OnSTileMoveStart -= DisableOnStart;
    }

    private void DisableOnStart(object sender, SGridAnimator.OnTileMoveArgs e){
        this.GetComponent<CompositeCollider2D>().enabled = false;
    }

    private void EnableOnEnd(object sender, SGridAnimator.OnTileMoveArgs e){
        this.GetComponent<CompositeCollider2D>().enabled = true;
    }*/

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>()){
            other.gameObject.GetComponent<Meltable>().AddLava();
            Debug.Log("added Lava");
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>())
            other.gameObject.GetComponent<Meltable>().RemoveLava();
    }
}
