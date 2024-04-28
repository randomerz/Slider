using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarriedItemLayerChanger : MonoBehaviour
{
    public int defaultLayer = 0;
    public int sortingLayer = 2;

    private void OnTriggerEnter2D()
    {
        Player.GetPlayerAction().SetItemSortingOrderInc(sortingLayer);
    }

    private void OnTriggerExit2D()
    {
        Player.GetPlayerAction().SetItemSortingOrderDec(defaultLayer);
    }
}
