using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : SGrid
{
    [Header("FactoryGrid")]
    [SerializeField] private ConditionChecker explosionChecker;
    [SerializeField] private PowerCrystal powerCrystal;
    [SerializeField] private ServerComputer serverComputer;

    public override void Init() {
        InitArea(Area.Factory);
        base.Init();
    }


    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Factory");
        UIEffects.FadeFromBlack();

        //SGrid.OnGridMove += (sender, e) => { Debug.Log(GetGridString()); };
    }

    public override void Save() 
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
    }

    public static bool IsInPast(GameObject entity)
    {
        return entity.transform.position.y < -50f;
    }

    public void ExplodeDoor()
    {
        CameraShake.Shake(1.0f, 1.0f);
        SaveSystem.Current.SetBool("doorExploded", true);
        explosionChecker.CheckConditions();
    }

    //THIS IS FOR DEBUG ONLY
    public static IEnumerator SendPlayerToPastDebugSequence(DebugUIManager duiManager)
    {
        duiManager.ES("6");
        yield return new WaitForSeconds(2.0f);
        (Current as FactoryGrid).powerCrystal.StartCrystalPoweredSequence();
        yield return new WaitForSeconds(2.0f);
        (Current as FactoryGrid).serverComputer.StartSendToPastEvent();
    }
}
