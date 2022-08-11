using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMachine : MonoBehaviour
{
    private int numGems;

    public void addGem(){
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
