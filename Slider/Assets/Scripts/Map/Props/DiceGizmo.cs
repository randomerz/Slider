using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceGizmo : MonoBehaviour
{
    public STile myStile;

    public int value;
    public Sprite[] sprites;
    //public Animator animator; // this is only based on Tree animator controller rn
    //Chen: Should the above be changed to the Dice animator controller or something?
    [SerializeField] private bool shouldDisableAtStart = true;

    private void Awake()
    {
        if (myStile == null)
        {
            FindSTile();
        }
        if (shouldDisableAtStart)
            gameObject.SetActive(false);
    }

    private void Update()
    {
        this.GetComponent<SpriteRenderer>().sprite = sprites[value - 1];
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
    public void changeValue(int num)
    {
        value = num;
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
