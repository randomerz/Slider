using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TreeBlackboard
{
    public static TreeBlackboard instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}
