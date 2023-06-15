using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactorySoftlockChecker : MonoBehaviour
{
    // Factory 1 tile + anchor
    // https://trello.com/c/Do3M1MdR/145-factory-1-tile-anchor
    public bool Softlock1()
    {
        bool isTile1Anchored = SGrid.Current.GetStile(1).hasAnchor;
        return (CheckGrid.contains(SGrid.GetGridString(), "#1#_###_###") && isTile1Anchored);
    }

    public void CheckSoftlock1(Condition c) => c.SetSpec(Softlock1());


    // Factory 2 tile + anchor
    // https://trello.com/c/1UziJGoY/249-factory-2-tile-anchor
    public bool Softlock2()
    {
        bool isTile2Anchored = SGrid.Current.GetStile(2).hasAnchor;
        return (CheckGrid.contains(SGrid.GetGridString(), "###_##2_###") && isTile2Anchored);
    }

    public void CheckSoftlock2(Condition c) => c.SetSpec(Softlock2());

    //// TODO: Factory closet softlocks
    //// https://trello.com/c/ewuJX8u1/350-factory-top-right-closet
    //public bool SoftlockCloset()
    //{
    //    return false;
    //}
}
