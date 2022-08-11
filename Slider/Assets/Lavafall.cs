using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lavafall : MonoBehaviour
{
    private STile sTile;
    [SerializeField] private GameObject fall;

    private void OnEnable() {
        sTile = GetComponentInParent<STile>();
        SGridAnimator.OnSTileMoveEnd += CheckLava;
        CheckLava();
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckLava;
    }

    public void CheckLava(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //if(e.stile == sTile)
           CheckLava();
    }

    public void CheckLava(){
        fall.SetActive(sTile.y > 1);
    }
}
