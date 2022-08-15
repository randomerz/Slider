using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    private bool walls = false;
    private bool floor = false;
    private bool wires = false;
    private bool giveslider = false;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartPoof()
    {
        Debug.Log("hello!");
        if (!giveslider && walls && floor && wires)
        {

            //To Code active animation
            //give slider
            SGrid.Current.ActivateSliderCollectible(5);
            giveslider = true;
        }
    }

    public void GetWalls()
    {
        walls = true;
    }
    public void GetFloor()
    {
        floor = true;
    }
    public void GetWires()
    {
        wires = true;
    }

}
