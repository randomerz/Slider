using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : SGrid
{
    [Header("FactoryGrid")]
    [SerializeField] private ConditionChecker explosionChecker;
    [SerializeField] private PowerCrystal powerCrystal;
    [SerializeField] private ServerComputer serverComputer;

    private bool playerInPast;

    public delegate void PlayerPastEvent(bool playerInPast);
    public static event PlayerPastEvent playerPastChanged;

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

    private void Update()
    {
        bool oldValue = playerInPast;
        playerInPast = FactoryGrid.IsInPast(Player.GetInstance().gameObject);
        if (oldValue != playerInPast)
        {
            playerPastChanged?.Invoke(playerInPast);
        }
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
