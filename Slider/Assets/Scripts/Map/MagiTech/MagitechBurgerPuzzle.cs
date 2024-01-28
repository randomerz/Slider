using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechBurgerPuzzle : MonoBehaviour
{
    public DesyncItem presentBurger;
    public DesyncItem pastBurger;
    private bool hasBurger;
    private bool hasDesyncBurger;
    private bool hasBurgerInPast;

    public void AddTrackers()
    {
        presentBurger.SetIsTracked(true);
        pastBurger.SetIsTracked(true);
    }

    public void RemoveTrackers()
    {
        presentBurger.SetIsTracked(false);
        pastBurger.SetIsTracked(false);
    }

    public void SetHasBurger(bool value)
    {
        hasBurger = value;
    }

    public void SetHasBurgerInPast(bool value)
    {
        hasBurgerInPast = value;
    }

    public void SetHasDesyncBurger(bool value)
    {
        hasDesyncBurger = value;
    }

    public void HasNormalBurger(Condition c)
    {
        c.SetSpec(hasBurger);
    }

    public void HasDesyncBurger(Condition c)
    {
        c.SetSpec(hasDesyncBurger);
    }

    public void HasBurgerInPast(Condition c)
    {
        c.SetSpec(hasBurgerInPast);
    }

    public void HasTwoBurgers(Condition c)
    {
        c.SetSpec(hasBurger && hasDesyncBurger);
    }
}
