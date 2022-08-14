using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour
{
    private int numGems;
    private STile sTile;
    public bool isPowered;

    private void Start() {
        sTile = GetComponentInParent<STile>();
    }

    public void addGem(){
        if(!isPowered)
            return;
        numGems++;
        if(numGems == 2)
            SGrid.Current.EnableStile(8);
    }

    public void RemoveGem(){
        numGems--;
    }

    public void ResetGems(){
        numGems = 0;
    }

    public void SetIsPowered(bool value){
        isPowered = value;
    }
}
