using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyManager : MonoBehaviour
{
  public GameObject buyPanel;
  public GameObject[] sliderButtons;

  public void whenSlidersBought(int sliderId)
  {
    if (sliderId != 4)
    {
      Debug.Log(OceanGrid.instance.totalCreditCount);
      if (OceanGrid.instance.totalCreditCount <= 0)
      {
        return; // does nothing if can't afford tile
      }
      OceanGrid.instance.totalCreditCount--;
    }
    SGrid.current.ActivateSliderCollectible(sliderId);
    sliderButtons[sliderId - 4].SetActive(false);
  }
}
