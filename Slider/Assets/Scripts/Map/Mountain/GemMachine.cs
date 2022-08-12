using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour
{
    private int numGems;
    private STile sTile;

    public WaterWheel waterWheel;

    private void Start() {
        sTile = GetComponentInParent<STile>();
    }

    public void addGem(){
        if(!waterWheel.IsDone())
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
}
