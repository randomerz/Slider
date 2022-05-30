using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBuyManager : MonoBehaviour
{
    public ShopManager shopManager;
    public GameObject[] sliderButtons;

    public void whenSlidersBought(int sliderId)
    {
        if (sliderId != 4 && sliderId != 1 && sliderId != 2 && sliderId != 3)
        {
            Debug.Log(shopManager.GetCredits());
            if (shopManager.GetCredits() <= 0)
            {
                return; // does nothing if can't afford tile
            }
            shopManager.SpendCredits(1);
        }
        SGrid.current.ActivateSliderCollectible(sliderId);
        sliderButtons[sliderId - 1].SetActive(false);
    }
}
