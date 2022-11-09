using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatTrick : MonoBehaviour
{
    public Collider2D detectionZone;

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
