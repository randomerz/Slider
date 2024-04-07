using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainCaveWall : MonoBehaviour, ISavable
{
    public List<GameObject> makeActiveOnExplosion = new List<GameObject>();
    public List<GameObject> deactivateOnExplosion = new List<GameObject>();
    public List<GameObject> moveOnExplosion = new List<GameObject>();
    public Minecart mc;
    public GameObject mcSpawn; //blowing up wall
    public GameObject mcLoadSpawn; //loading from entering scene after wall blown up
    public bool didBlowUp = false;
    public ExplodableRock explodableRock;

    public void BlowUpCaveWall()
    {
        if(didBlowUp)
            return;
        didBlowUp = true;
        explodableRock?.ExplodeRock();
        CameraShake.Shake(1f, 3.5f);
        AudioManager.Play("Slide Explosion");

        foreach (GameObject go in makeActiveOnExplosion)
            go.SetActive(true);
        foreach (GameObject go in deactivateOnExplosion)
            go.SetActive(false);
        foreach (GameObject go in moveOnExplosion)
            go.transform.position += new Vector3(100, 0, 0);
        StartCoroutine(WaitForMCSpawn());
    }

    private IEnumerator WaitForMCSpawn()
    {
        
        mc.gameObject.SetActive(true);
       // mc.UpdateState("RepairParts");
        yield return new WaitForSeconds(2);
        mc.SnapToRail(mcSpawn.transform.position, 1);
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
        if(didBlowUp)
        {
            foreach (GameObject go in moveOnExplosion)
                go.transform.position += new Vector3(100, 0, 0);
        }
        if(didBlowUp && mc != null) {
            mc.gameObject.SetActive(true);
            mc.SnapToRail(mcLoadSpawn.transform.position, 2);
        }
    }
}
