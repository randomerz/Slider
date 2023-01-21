using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class Bin : Box
{ 
    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>();
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
        //add the shape
        recievedShapes[path] = shape;

        if( shape != null)
        {
            //broadcast a shape has been made
            print("bin: " + shape.name);
        }

        foreach (Path p in recievedShapes.Keys)
        {
            if (recievedShapes[p] != null)
            {
                print(recievedShapes[p].name);
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
