using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsSpecialEvents : MonoBehaviour
{
    public JungleBlobPathController blobPathController;
    public Shape shape;
    public JungleBox target;
    

    private void Start()
    {
        SetupBlobs();
    }

    
    private void SetupBlobs()
    {
        blobPathController.EnableMarching(Direction.RIGHT, shape, target);
    }
}
