using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CatTrick : MonoBehaviour
{
    public Collider2D detectionZone;

    public TextMeshProUGUI text;
    public TextMeshProUGUI backgroundText;

    public Item mittens;
    public Item autumn;
    public Item tofu;

    private bool hasMittens;
    private bool hasAutumn;
    private bool hasTofu;

    private ContactFilter2D contactFilter;

    private void Awake() 
    {
        contactFilter = new ContactFilter2D();
    }

    private void Update() 
    {
        UpdateCollection();
    }

    private void UpdateCollection()
    {
        List<Collider2D> results = new List<Collider2D>();
        detectionZone.OverlapCollider(contactFilter, results);

        foreach (Collider2D c in results)
        {
            Item i = c.GetComponent<Item>();
            if (i != null)
            {
                hasMittens = i.itemName == mittens.itemName || hasMittens;
                hasAutumn = i.itemName == autumn.itemName || hasAutumn;
                hasTofu = i.itemName == tofu.itemName || hasTofu;
            }
        }

        UpdateText();
    }

    public void UpdateText()
    {
        int count = 0;
        if (hasMittens) count += 1;
        if (hasAutumn) count += 1;
        if (hasTofu) count += 1;

        text.text = $"{count}/3";
        backgroundText.text = $"{count}/3";

        text.gameObject.SetActive(0 < count && count < 3);
        backgroundText.gameObject.SetActive(0 < count && count < 3);
    }

    public void CheckMittens(Condition c)
    {
        c.SetSpec(hasMittens);
    }

    public void CheckAutumn(Condition c)
    {
        c.SetSpec(hasAutumn);
    }

    public void CheckTofu(Condition c)
    {
        c.SetSpec(hasTofu);
    }
}
