using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class Bin : Box
{ 
    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>();
    public GameObject shape1;
    public GameObject shape2;
    private int numShapes = 0;

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += OnSTileMoveEarly;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= OnSTileMoveEarly;
    }

    private void OnSTileMoveEarly(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //remove all shapes
        currentShape = null;
        numShapes = 0;
        shape1.gameObject.SetActive(false);
        shape2.gameObject.SetActive(false);
        print("no shape");
    }

    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        //add the shape
        recievedShapes[path] = shape;

        if( shape != null)
        {
            //broadcast a shape has been made
            print("bin: " + shape.name);
            numShapes++;
        } else
        {
            numShapes--;
        }

        foreach (Path p in recievedShapes.Keys)
        {
            if (recievedShapes[p] != null)
            {
                if (numShapes == 1)
                {
                    shape1.GetComponentInChildren<SpriteRenderer>().sprite = recievedShapes[p].sprite;
                    shape1.gameObject.SetActive(true);
                } else { 
                    shape2.GetComponentInChildren<SpriteRenderer>().sprite = recievedShapes[p].sprite;
                    shape2.gameObject.SetActive(true);
                }
                //print(recievedShapes[p].name);
            } else
            {
                if (numShapes == 0)
                {
                    shape1.gameObject.SetActive(false);
                } else
                {
                    shape2.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Blob blob = collider.gameObject.GetComponent<Blob>();
        if (blob != null)
        {
            blob.JumpIntoBin();
        }
    }
}
