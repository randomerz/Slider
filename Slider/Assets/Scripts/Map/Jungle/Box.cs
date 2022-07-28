using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    List<Path> paths = new List<Path>();
    Path currentPath;
    List<Shape> shapes;
    List<Shape> currentShape;

    //some way to keep direction here

    // Start is called before the first frame update
    void Start()
    {
        //start a path here in a default direction
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Shape CreateShape()
    {
        return null;
    }
    public void Rotate()
    {

    }
}
