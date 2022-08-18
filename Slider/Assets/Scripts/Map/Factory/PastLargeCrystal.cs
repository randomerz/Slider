using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastLargeCrystal : ConductiveElectricalNode
{
    [Header("Past Large Crystal")]
    [SerializeField] private PlayerPositionChanger ppChanger;
    [SerializeField] private PowerCrystal powerCrystal;

    private bool CrystalHasEnoughPower => Powered && powerPathPrevs.Keys.Count >= 2;

    public void CheckCrystalHasEnoughPower(Condition cond)
    {
        cond.SetSpec(CrystalHasEnoughPower);
    }

    public void BTTFStarter()
    {
        CameraShake.ShakeIncrease(3.0f, 0.5f);
        UIEffects.FadeToWhite(() =>
        {
            StartCoroutine(BackToTheFuture());
        }, 3.0f, false);
    }

    private IEnumerator BackToTheFuture()
    {
        yield return new WaitForSeconds(2.0f);
        SaveSystem.Current.SetBool("DidBTTF", true);
        powerCrystal.TurnEverythingBackOn();
        ppChanger.UPPTransform();
        UIEffects.FadeFromWhite();
    }
}
