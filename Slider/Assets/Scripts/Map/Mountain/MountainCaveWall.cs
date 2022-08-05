using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainCaveWall : MonoBehaviour
{
    public GameObject wall;
    public List<GameObject> makeActive = new List<GameObject>();
    public Minecart mc;
    public GameObject mcSpawn;

    public void BlowUpCaveWall()
    {
        wall.SetActive(false);
        CameraShake.Shake(1f, 3.5f);
        AudioManager.Play("Slide Explosion");
        foreach (GameObject go in makeActive)
            go.SetActive(true);
        mc.SnapToRail(mcSpawn.transform.position, 0);
        mc.UpdateState("RepairParts");
        mc.StartMoving();
    }
}
