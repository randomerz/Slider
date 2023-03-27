using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopCoinToBeHadManager : MonoBehaviour, ISavable
{
    public TextMeshProUGUI talkText;
    public TextMeshProUGUI coinToBeHadText;

    public TextMeshProUGUI business;
    public TextMeshProUGUI shipwreck;
    public TextMeshProUGUI magicalSpells;
    public TextMeshProUGUI theVeil;
    public TextMeshProUGUI tangledUp;
    public TextMeshProUGUI aBuddingRomance;
    public TextMeshProUGUI eruption;

    TextMeshProUGUI[] texts;

    private bool[] isNew = {
        true,
        true,
        true,
        true,
        true,
        true,
        true
    };

    private void Awake() {
        
        texts = new TextMeshProUGUI[] {
            business,
            shipwreck,
            magicalSpells,
            theVeil,
            tangledUp,
            aBuddingRomance,
            eruption,
        };
    }

    public void Save()
    {
        for (int i = 0; i < isNew.Length; i++)
        {
            SaveSystem.Current.SetBool($"oceanCoinHintsIsNew{i}", isNew[i]);
        }
    }

    public void Load(SaveProfile profile)
    {
        for (int i = 0; i < isNew.Length; i++)
        {
            isNew[i] = profile.GetBool($"oceanCoinHintsIsNew{i}", true);
        }
    }

    public void UpdateButtons()
    {
        int numEnabled = 0;

        // Shipwreck
        if (SGrid.Current.GetStile(4).isTileActive && !PlayerInventory.Contains("Treasure Chest") &&
            numEnabled < 3)
        {
            shipwreck.transform.parent.gameObject.SetActive(true);
            numEnabled += 1;
        }
        else
            shipwreck.transform.parent.gameObject.SetActive(false);
        
        // Magical Spells
        if (SGrid.Current.GetStile(5).isTileActive && !PlayerInventory.Contains("Magical Gem") &&
            numEnabled < 3)
        {
            magicalSpells.transform.parent.gameObject.SetActive(true);
            numEnabled += 1;
        }
        else
            magicalSpells.transform.parent.gameObject.SetActive(false);
        
        // The Veil
        if (SGrid.Current.GetStile(6).isTileActive && SGrid.Current.GetStile(7).isTileActive && 
            !PlayerInventory.Contains("Mushroom") && numEnabled < 3)
        {
            theVeil.transform.parent.gameObject.SetActive(true);
            numEnabled += 1;
        }
        else
            theVeil.transform.parent.gameObject.SetActive(false);
        
        // Tangled Up
        if (SGrid.Current.GetStile(1).isTileActive && SGrid.Current.GetStile(3).isTileActive && 
            SGrid.Current.GetStile(4).isTileActive && SGrid.Current.GetStile(8).isTileActive && 
            SGrid.Current.GetStile(9).isTileActive && !PlayerInventory.Contains("Golden Fish") &&
            numEnabled < 3)
        {
            tangledUp.transform.parent.gameObject.SetActive(true);
            numEnabled += 1;
        }
        else
            tangledUp.transform.parent.gameObject.SetActive(false);

        // A Budding Romance
        if (!PlayerInventory.Contains("Rose") && numEnabled < 3)
        {
            aBuddingRomance.transform.parent.gameObject.SetActive(true);
            numEnabled += 1;
        }
        else
            aBuddingRomance.transform.parent.gameObject.SetActive(false);
        
        // Eruption
        if (SGrid.Current.GetStile(3).isTileActive && SGrid.Current.GetStile(4).isTileActive && 
            SGrid.Current.GetStile(5).isTileActive && SGrid.Current.GetStile(8).isTileActive && 
            SGrid.Current.GetStile(9).isTileActive && !PlayerInventory.Contains("Rock") && numEnabled < 3)
        {
            eruption.transform.parent.gameObject.SetActive(true);
            numEnabled += 1;
        }
        else
            eruption.transform.parent.gameObject.SetActive(false);
    }

    public void UpdateTexts()
    {
        if (texts == null) Awake(); // bc of initialization order problems

        bool anyTruers = false;

        for (int i = 0; i < texts.Length; i++)
        {
            if (isNew[i] && texts[i].transform.parent.gameObject.activeSelf) 
            {
                texts[i].text = texts[i].text.Replace("*", "") + "*";
                anyTruers = true;
            }
        }

        talkText.text = talkText.text.Replace("*", "");
        coinToBeHadText.text = coinToBeHadText.text.Replace("*", "");

        // if any are new add a *
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
            texts[index].text = texts[index].text.Replace("*", "");

            UpdateTexts();
        }
    }
}
