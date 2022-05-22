using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveOnPowered : MonoBehaviour
{
    public void OnHandlerSetActive(ElectricalNode.OnPoweredArgs e)
    {
        gameObject.SetActive(e.powered);
    }
}

//This is it? This is the script I've heard so much about and feared all this time? Yes it is.