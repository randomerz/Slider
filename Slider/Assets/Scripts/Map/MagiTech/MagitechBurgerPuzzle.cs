using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechBurgerPuzzle : MonoBehaviour
{
    private bool hasBurger;
    private bool hasDesyncBurger;
    private bool hasBurgerInPast;

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
