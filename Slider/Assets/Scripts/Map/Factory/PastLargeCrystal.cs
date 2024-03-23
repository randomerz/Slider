using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastLargeCrystal : ElectricalNode
{
    [Header("Past Large Crystal")]
    [SerializeField] private PlayerPositionChanger ppChanger;
    [SerializeField] private PowerCrystal powerCrystal;

    [SerializeField] private GameObject crystalBTTFParticles;
    private List<GameObject> particles = new List<GameObject>();

    public void CheckCrystalHasEnoughPower(Condition cond)
    {
        cond.SetSpec(CrystalHasEnoughPower());
    }

    private bool CrystalHasEnoughPower()
    {
        int count = 0;
        foreach (ElectricalNode node in _incomingNodes)
        {
            count += node.Powered ? 1 : 0;
        }

        return Powered && count >= 2;
    }

    public void BTTFStarter()
    {
        StartCoroutine(BTTFAnimation());
    }

    private IEnumerator BTTFAnimation()
    {
        StartCoroutine(BTTFParticleAnimation(7));

        yield return new WaitForSeconds(1);

        CameraShake.ShakeIncrease(6.0f, 0.5f);

        yield return new WaitForSeconds(3);

        UIEffects.FadeToWhite(() =>
        {
            StartCoroutine(BackToTheFuture());
        }, 3.0f, false);
    }

    // this could be optimized a lot
    private IEnumerator BTTFParticleAnimation(int numRecur)
    {
        if (numRecur == 0)
            yield break;

        for (int i = 0; i < 4; i++)
        {
            particles.Add(GameObject.Instantiate(crystalBTTFParticles, transform.position + GetRandomPosition(), Quaternion.identity, transform));

            yield return new WaitForSeconds(0.25f);
        }

        StartCoroutine(BTTFParticleAnimation(numRecur - 1));

        for (int i = 0; i < 32; i++)
        {
            particles.Add(GameObject.Instantiate(crystalBTTFParticles, transform.position + GetRandomPosition(), Quaternion.identity, transform));

            yield return new WaitForSeconds(0.25f);
        }
    }
    
    private Vector3 GetRandomPosition()
    {
        float r = Random.Range(0f, 8f);
        float t = Random.Range(0f, 360f);

        return new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
    }

    private IEnumerator BackToTheFuture()
    {
        yield return new WaitForSeconds(2.0f);

        SaveSystem.Current.SetBool("factoryDidBTTF", true);
        powerCrystal.TurnEverythingBackOn();
        FactoryTimeManager.SpawnPlayerInPresent();

        // Make sure player has sliders up to 8 in case of weird bugs/skips
        if (!PlayerInventory.Contains("Slider 7", Area.Factory)) 
            SGrid.Current.GetCollectible("Slider 7").DoOnCollect();
        if (!PlayerInventory.Contains("Slider 8", Area.Factory)) 
            SGrid.Current.GetCollectible("Slider 8").DoOnCollect();
        
        SGrid.Current.ActivateSliderCollectible(9);

        foreach (GameObject go in particles)
        {
            go.SetActive(false);
        }

        // if the bool is true that means we did kill him, so we want to give achievement if it is false
        if (!SaveSystem.Current.GetBool("ChadSrPuzzleComplete"))
        {
            AchievementManager.SetAchievementStat("sparedChadSr", 1);
        }

        StopAllCoroutines();
    }
}
