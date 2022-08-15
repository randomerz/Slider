using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastLargeCrystal : ConductiveElectricalNode
{
    [Header("Past Large Crystal")]
    [SerializeField] private PlayerPositionChanger ppChanger;

    public void CheckCrystalHasEnoughPower(Condition cond)
    {
        cond.SetSpec(Powered && powerRefs >= 2);
    }

    public void BTTFStarter()
    {
        CameraShake.ShakeIncrease(1.0f, 0.5f);
        UIEffects.FadeToWhite(() =>
        {
            StartCoroutine(BackToTheFuture());
        }, 2.0f, false);
    }

    private IEnumerator BackToTheFuture()
    {
        yield return new WaitForSeconds(2.0f);
        SaveSystem.Current.SetBool("DidBTTF", true);
        ppChanger.UPPTransform();
        UIEffects.FadeFromWhite();
    }
}
