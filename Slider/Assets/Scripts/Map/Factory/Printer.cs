using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    [SerializeField]
    public GameObject wallObject;
    public GameObject floorObject;
    public GameObject wireObject;

    private bool giveslider = false;
    // Start is called before the first frame update
    // Update is called once per frame
    private bool walls = false;
    private bool floor = false;
    private bool wires = false;
    void Awake()
    {
        CheckParts();
    }
    void Update()
    {
        
    }

    public void StartPoof()
    {
        if (!giveslider && walls && floor && wires)
        {

            //To Code active animation
            //give slider
            SGrid.Current.ActivateSliderCollectible(5);
            giveslider = true;
        }
    }

    public void CheckParts()
    {
        string operatorMessage = "";
        walls = PlayerInventory.Contains("Walls");
        floor = PlayerInventory.Contains("Floor");
        wires = PlayerInventory.Contains("Wires");
        SetActives();
        if (!floor && !walls && !wires)
        {
            operatorMessage = "This printer can print the next tile! Bring these three parts: the floor, the walls, and the wires.";
        }
        else if (floor && walls && wires)
        {
            operatorMessage = "You have all the parts! You're ready to print!";
        }
        else
        {
            List<string> mlist = new List<string>();
            operatorMessage = "It still needs";
            if (!floor)
            {
                mlist.Add(" the floor");
            }
            if (!walls)
            {
                mlist.Add(" the walls");
            }
            if (!wires)
            {
                mlist.Add(" the wires");
            }
            operatorMessage += string.Join(",", mlist) + ".";
        }
        SaveSystem.Current.SetString("FactoryPrinterParts", operatorMessage);
    }

    private void SetActives()
    {
        wallObject.SetActive(walls);
        floorObject.SetActive(floor);
        wireObject.SetActive(wires);
    }
}
