using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsSpecialEvents : MonoBehaviour
{
    public JungleBlobPathController blobPathController;
    public Shape shape;
    public JungleBox target;
    public Transform mcSpawn;
    public Minecart minecart;

    private void Start()
    {
        SetupBlobs();
    }
    
    private void SetupBlobs()
    {
        blobPathController.EnableMarching(Direction.RIGHT, shape, target);
    }

    public void StartMinecart()
    {
        minecart.SnapToRail(mcSpawn.position, 1);
        minecart.StartMoving();
    }

}
