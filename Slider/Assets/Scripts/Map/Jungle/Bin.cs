using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class Bin : Box
{


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
        print("no shape");
    }

    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        if (shape == null)
        {
            print("bin: no shape");
        }
        else
        {
            //broadcast the shape has been made
            print("bin: " + shape.name);
            currentShape =  shape;
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
