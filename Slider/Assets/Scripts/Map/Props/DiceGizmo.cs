using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceGizmo : MonoBehaviour
{
    public STile myStile;
    public int value = 1;
    //public Animator animator; // this is only based on Tree animator controller rn
    //Chen: Should the above be changed to the Dice animator controller or something?

    private void Awake()
    {
        if (myStile == null)
        {
            FindSTile();
        }
    }

    private void OnEnable()
    {
        if (myStile != null)
            myStile.onChangeMove += OnStileChangeDir;
    }

    private void OnDisable()
    {
        if (myStile != null)
            myStile.onChangeMove -= OnStileChangeDir;
    }

    public void OnStileChangeDir(object sender, STile.STileMoveArgs e)
    {
        //Debug.Log("Dice stuff");
        //Debug.Log(e.moveDir);
        if (e.moveDir != Vector2.zero)
        {
            value++;
            if (value == 7)
            {
                value = 1;
            }
        }
        //Debug.Log(value);
        // Debug.Log("Updated!");
        //Chen: Upadte dice dialogue here maybe?
    }



    private void FindSTile()
    {
        Transform curr = transform;
        int i = 0;
        while (curr.parent != null && i < 100)
        {
            if (curr.GetComponent<STile>() != null)
            {
                myStile = curr.GetComponent<STile>();
                return;
            }

            // Debug.Log(curr.name);
            curr = curr.parent;
            i += 1;
        }

        if (i == 100)
            Debug.LogWarning("something went wrong in finding stile!");
    }
}
