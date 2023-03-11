using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleTurnInAccepter : MonoBehaviour
{

    public Shape neededShape;
    //might wanna refactor this to just auto find it
    public JungleShapeManager jm;

    // on trigger call the jm to see what shape the player and pass the shape you need then do stuff if it is the same
    //set flag as tru and remove shape from player
    // flags are in save profiles

    public void OnShapeTriggerEnter()
    {
        bool removed = jm.TurnInShape(neededShape);
    }
}
