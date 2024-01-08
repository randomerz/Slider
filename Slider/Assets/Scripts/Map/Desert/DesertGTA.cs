using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGTA : MonoBehaviour, ISavable
{
    [SerializeField] private bool explodedWall = false;
    [SerializeField] private bool explosivesPlaced = false;
    [SerializeField] private List<GameObject> placableExplosives;
    [SerializeField] private List<GameObject> removeAfterExplosion;
    [SerializeField] private GameObject explosivesPlaceConditional;
    public PlayerConditionals bombSignConditional;


    public void PlaceExplosives()
    {
        if(explosivesPlaced) return;

        explosivesPlaced = true;
        foreach(GameObject g in placableExplosives)
        {
            g.SetActive(true);
        }
        AudioManager.Play("Hat Click");
        bombSignConditional.DisableConditionals();
    }

    public void ExplodeCasinoWall()
    {
        StartCoroutine(ExplodeCasinoWallCoroutine());
    }

    private IEnumerator ExplodeCasinoWallCoroutine()
    {
        yield return null;
        EndExplosion();
    }

    public void EndExplosion(bool fromSave = false)
    {
        foreach(GameObject g in removeAfterExplosion)
        {
            g.SetActive(false);
        }
        explodedWall = true;
    }

    public void Save()
    {
        throw new System.NotImplementedException();
    }

    public void Load(SaveProfile profile)
    {
        explodedWall = profile.GetBool("DesertBlewUpCasinoWall");
        if(explodedWall)
        {
            EndExplosion(true);
        }
    }

    public void BlewUpCasinoWall(Condition c)
    {
        c.SetSpec(explodedWall);
    }
}
