using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class Bin : Box
{ 
    public static System.EventHandler<JungleBinArgs> OnBinRecieveShape;

    public class JungleBinArgs : System.EventArgs
    {
        public Shape shape;
    }

    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>();
    public ShapePlacer shapePlacer1;
    public ShapePlacer shapePlacer2;

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
        shapePlacer1.Stop();
        shapePlacer2.Stop();
        //print("no shape");
    }

    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        //add the shape
        recievedShapes[path] = shape;

        JungleRecipeBookSave.IncrementNumberCreated(shape);

        OnBinRecieveShape?.Invoke(this, new JungleBinArgs { shape = shape });

        shapePlacer1.Stop();
        shapePlacer2.Stop();

        int numShapes = 0;
        foreach (Path p in recievedShapes.Keys)
        {
            if (recievedShapes[p] != null)
            {
                if (numShapes == 0)
                {
                    shapePlacer1.gameObject.SetActive(true);
                    shapePlacer1.Place(recievedShapes[p]);
                    numShapes++;
                } else
                {
                    shapePlacer2.gameObject.SetActive(true);
                    shapePlacer2.Place(recievedShapes[p]);
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
