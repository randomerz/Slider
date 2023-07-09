using UnityEngine;

public class LavaParticleTile : MonoBehaviour
{
    public GameObject[] particleSystems = new GameObject[4]; //0 1 2 3 = E N W S

    public void EnableParticles(int groupNum)
    {
        particleSystems[groupNum].SetActive(true);
    }

    public void DisableParticles(int groupNum)
    {
        particleSystems[groupNum].SetActive(false);
    }
}
