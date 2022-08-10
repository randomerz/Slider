using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    //C: this is overcomplicated because OnTriggerEnter/Exit
    //are inconsistent. Deal with it.

    /*private List<Meltable> meltList = new List<Meltable>();
    public STile sTile;

    private void OnEnable() {
        SGridAnimator.OnSTileMoveEndEarly += CheckLavaOnMoveEnd; //Done so this will update before melting/freezing checks;
        sTile = GetComponentInParent<MountainSTile>();
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEndEarly -= CheckLavaOnMoveEnd;
    }

    private void CheckLavaOnMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Collider2D other in Colliders.)
    }*/
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>())
            other.gameObject.GetComponent<Meltable>().AddLava();
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.GetComponent<Meltable>())
            other.gameObject.GetComponent<Meltable>().RemoveLava();
    }
}
