using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopCoinToBeHadManager : MonoBehaviour
{
    public TextMeshProUGUI talkText;
    public TextMeshProUGUI coinToBeHadText;

    public TextMeshProUGUI business;
    public TextMeshProUGUI shipwreck;
    public TextMeshProUGUI downWithTheShip;
    public TextMeshProUGUI theVeil;
    public TextMeshProUGUI tangledUp;
    public TextMeshProUGUI eruption;

    private bool[] isNew = { // TODO: serialize
        true,
        true,
        true,
        true,
        true,
        true
    };

    public void UpdateButtons()
    {
        // Shipwreck
        if (SGrid.current.GetStile(4).isTileActive && !PlayerInventory.Contains("Treasure Chest"))
        {
            shipwreck.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            shipwreck.transform.parent.gameObject.SetActive(false);
        }
        
        // Down with the Ship
        if (SGrid.current.GetStile(5).isTileActive && !PlayerInventory.Contains("Treasure Map"))
        {
            downWithTheShip.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            downWithTheShip.transform.parent.gameObject.SetActive(false);
        }
        
        // The Veil
        if (SGrid.current.GetStile(6).isTileActive && SGrid.current.GetStile(7).isTileActive && 
            !PlayerInventory.Contains("Mushroom"))
        {
            theVeil.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            theVeil.transform.parent.gameObject.SetActive(false);
        }
        
        // Tangled Up
        if (SGrid.current.GetStile(1).isTileActive && SGrid.current.GetStile(3).isTileActive && 
            SGrid.current.GetStile(4).isTileActive && SGrid.current.GetStile(8).isTileActive && 
            SGrid.current.GetStile(9).isTileActive && !PlayerInventory.Contains("Golden Fish"))
        {
            tangledUp.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            tangledUp.transform.parent.gameObject.SetActive(false);
        }
        
        // Eruption
        if (SGrid.current.GetStile(3).isTileActive && SGrid.current.GetStile(4).isTileActive && 
            SGrid.current.GetStile(5).isTileActive && SGrid.current.GetStile(8).isTileActive && 
            SGrid.current.GetStile(9).isTileActive && !PlayerInventory.Contains("Rock"))
        {
            eruption.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            eruption.transform.parent.gameObject.SetActive(false);
        }
    }

    public void UpdateTexts()
    {
        bool anyTruers = false;
        if (isNew[0] && business.transform.parent.gameObject.activeSelf) 
        {
            business.text = business.text.Replace("*", "") + "*";
            anyTruers = true;
        }
        if (isNew[1] && shipwreck.transform.parent.gameObject.activeSelf) 
        {
            shipwreck.text = shipwreck.text.Replace("*", "") + "*";
            anyTruers = true;
        }
        if (isNew[2] && downWithTheShip.transform.parent.gameObject.activeSelf) 
        {
            downWithTheShip.text = downWithTheShip.text.Replace("*", "") + "*";
            anyTruers = true;
        }
        if (isNew[3] && theVeil.transform.parent.gameObject.activeSelf) 
        {
            theVeil.text = theVeil.text.Replace("*", "") + "*";
            anyTruers = true;
        }
        if (isNew[4] && tangledUp.transform.parent.gameObject.activeSelf) 
        {
            tangledUp.text = tangledUp.text.Replace("*", "") + "*";
            anyTruers = true;
        }
        if (isNew[5] && eruption.transform.parent.gameObject.activeSelf) 
        {
            eruption.text = eruption.text.Replace("*", "") + "*";
            anyTruers = true;
        }

        talkText.text = talkText.text.Replace("*", "");
        coinToBeHadText.text = coinToBeHadText.text.Replace("*", "");

        if (anyTruers)
        {
            talkText.text += "*";
            coinToBeHadText.text += "*";
        }
    }

    public void UnNew(int index)
    {
        if (isNew[index])
        {
            isNew[index] = false;

            switch (index)
            {
                case 0:
                    business.text = business.text.Replace("*", "");
                    break;
                case 1:
                    shipwreck.text = shipwreck.text.Replace("*", "");
                    break;
                case 2:
                    downWithTheShip.text = downWithTheShip.text.Replace("*", "");
                    break;
                case 3:
                    theVeil.text = theVeil.text.Replace("*", "");
                    break;
                case 4:
                    tangledUp.text = tangledUp.text.Replace("*", "");
                    break;
                case 5:
                    eruption.text = eruption.text.Replace("*", "");
                    break;
            }

            UpdateTexts();
        }
    }
}
