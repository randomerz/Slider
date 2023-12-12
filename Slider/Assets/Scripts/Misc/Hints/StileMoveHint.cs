using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StileMoveHint : MonoBehaviour
{
    public STile stile;
    public bool shouldCheckAnySTile;
    public UnityEvent OnStileMove;

    //C: Made for hints, but generally flexible in allowing you to call events when a certain Stile is moved
    private void OnEnable()
    {
        if (stile != null)
            stile.onChangeMove += StileMove;
        if (shouldCheckAnySTile)
            SGrid.OnGridMove += StileMove;
    }

    private void OnDisable()
    {
        if (stile != null)
            stile.onChangeMove -= StileMove;
        if (shouldCheckAnySTile)
            SGrid.OnGridMove -= StileMove;
    }

    public void StileMove(object sender, System.EventArgs e)
    {
        OnStileMove.Invoke();
    }
}
