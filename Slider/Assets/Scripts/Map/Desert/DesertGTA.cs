using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGTA : MonoBehaviour, ISavable
{
    private bool explodedWall = false;
    private bool explosivesPlaced = false;
    [SerializeField] private List<GameObject> placableExplosives;
    [SerializeField] private GameObject laserCollider;
    public List<GameObject> removeAfterExplosion;
    public PlayerConditionals bombSignConditional;
    private bool isExploding;

    
    public void Save()
    {
        SaveSystem.Current.SetBool("DesertBlewUpCasinoWall", explodedWall);
    }

    public void Load(SaveProfile profile)
    {
        explodedWall = profile.GetBool("DesertBlewUpCasinoWall");
        if(explodedWall)
        {
            EndExplosion(true);
        }
    }


    public void PlaceExplosives()
    {
        if(explosivesPlaced) return;
        //if(!PlayerInventory.Contains("Explosives", Area.Military)) return;

        explosivesPlaced = true;
        foreach(GameObject g in placableExplosives)
        {
            g.SetActive(true);
        }
        AudioManager.Play("Hat Click");
        bombSignConditional.DisableConditionals();
        laserCollider.SetActive(true);
    }

    public void ExplodeCasinoWall()
    {
        StartCoroutine(ExplodeCasinoWallCoroutine());
    }

    private IEnumerator ExplodeCasinoWallCoroutine()
    {
        isExploding = true;
        AudioManager.Play("Slide Explosion");
        yield return new WaitForSeconds(2);
        EndExplosion();
    }

    public void EndExplosion(bool fromSave = false)
    {
        foreach(GameObject g in removeAfterExplosion)
        {
            g.SetActive(false);
        }
        foreach(GameObject g in placableExplosives)
        {
            g.SetActive(false);
        }
        laserCollider.SetActive(false);
        explodedWall = true;
        isExploding = false;
        SaveSystem.Current.SetBool("DesertBlewUpCasinoWall", true);
    }

    public void BlewUpCasinoWall(Condition c)
    {
        c.SetSpec(explodedWall);
    }

    public void IsBlowingUpCasinoWall(Condition c)
    {
        c.SetSpec(isExploding);
    }
}
