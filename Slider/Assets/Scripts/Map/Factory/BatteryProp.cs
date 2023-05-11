using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryProp : MonoBehaviour
{
    [SerializeField] private SpriteSwapper spriteSwapper;
    

    public void SetActive(bool value)
    {
        if (value)
        {
            spriteSwapper.TurnOn();
        }
        else
        {
            spriteSwapper.TurnOff();
        }

    }
}
