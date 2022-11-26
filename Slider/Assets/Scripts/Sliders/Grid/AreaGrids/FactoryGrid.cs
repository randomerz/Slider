using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : SGrid
{
    [Header("FactoryGrid")]
    [SerializeField] private ConditionChecker explosionChecker;
    [SerializeField] private PowerCrystal powerCrystal;

    public static bool PlayerInPast => IsInPast(Player.GetInstance().gameObject);

    public override void Init() {
        InitArea(Area.Factory);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();

        if (!PlayerInPast)
        {
            AudioManager.PlayMusic("Factory");
        }
        UIEffects.FadeFromBlack();

        //FactoryTimeManager.EnableAnchorsInTime(PlayerInPast);
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
        SaveSystem.Current.SetBool("factoryDoorExploded", true);
        explosionChecker.CheckConditions();
    }
}
