using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastLargeCrystal : ElectricalNode
{
    [Header("Past Large Crystal")]
    [SerializeField] private PlayerPositionChanger ppChanger;
    [SerializeField] private PowerCrystal powerCrystal;
    [SerializeField] private string crystalBTTFParticleName;

    private GameObject crystalBTTFParticles;
    private List<GameObject> particles = new List<GameObject>();

    protected override void Awake() {
        base.Awake();

        crystalBTTFParticles = Resources.Load<GameObject>(crystalBTTFParticleName);
        if (crystalBTTFParticles == null)
            Debug.LogError("Couldn't load particles!");
    }

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

        SaveSystem.Current.SetBool("DidBTTF", true);
        powerCrystal.TurnEverythingBackOn();
        FactoryTimeManager.SpawnPlayerInPresent();

        foreach (GameObject go in particles)
        {
            go.SetActive(false);
        }
        StopAllCoroutines();
    }
}
