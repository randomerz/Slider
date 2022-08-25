using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainCaveWall : MonoBehaviour, ISavable
{
    public List<GameObject> makeActiveOnExplosion = new List<GameObject>();
    public List<GameObject> deactivateOnExplosion = new List<GameObject>();
    public Minecart mc;
    public GameObject mcSpawn;
    public bool didBlowUp = false;

    public void BlowUpCaveWall()
    {
        if(didBlowUp)
            return;
        didBlowUp = true;

        CameraShake.Shake(1f, 3.5f);
        AudioManager.Play("Slide Explosion");

        foreach (GameObject go in makeActiveOnExplosion)
            go.SetActive(true);
        foreach (GameObject go in deactivateOnExplosion)
            go.SetActive(false);

        mc.gameObject.SetActive(true);
        mc.SnapToRail(mcSpawn.transform.position, 1);
        mc.UpdateState("RepairParts");
        mc.StartMoving();
    }

    public void CheckBlownUp(Condition c)
    {
        c.SetSpec(didBlowUp);
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("cavesMCWallExploded", didBlowUp);
    }

    public void Load(SaveProfile profile)
    {
        didBlowUp = profile.GetBool("cavesMCWallExploded");
        foreach (GameObject go in makeActiveOnExplosion)
            go.SetActive(didBlowUp);
        foreach (GameObject go in deactivateOnExplosion)
            go.SetActive(!didBlowUp);
    }
}
