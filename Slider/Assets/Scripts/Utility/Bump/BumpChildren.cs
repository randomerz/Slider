using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BumpChildren : MonoBehaviour
{
    public List<Bump> children = new();
    public bool shouldFindChildrenAutomatically = true;

    void Awake()
    {
        if (shouldFindChildrenAutomatically)
        {
            Bump[] c = GetComponentsInChildren<Bump>();
            children.AddRange(c);
        }
    }
    
    public void DoBump(bool withSound = true, Action callback = null)
    {
        if (withSound)
        {
            AudioManager.PlayWithVolume("Hat Click", 0.05f);
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].DoBump(i == 0 ? callback : null);
        }
    }
}
