using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class FlashWhiteChildren : MonoBehaviour, IFlashWhite
{
    public List<IFlashWhite> children = new();
    public bool shouldFindChildrenAutomatically = true;

    void Awake()
    {
        if (shouldFindChildrenAutomatically)
        {
            IFlashWhite[] c = GetComponentsInChildren<IFlashWhite>();
            children.AddRange(c);
            children.Remove(this);
        }
    }
    
    public void Flash(int n, Action callback = null)
    {
        for (int i = 0; i < children.Count; i++)
        {
            children[i].Flash(n, i == 0 ? callback : null);
        }
    }
}
