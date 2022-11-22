using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : SGrid
{
    [Header("FactoryGrid")]
    [SerializeField] private ConditionChecker explosionChecker;
    [SerializeField] private PowerCrystal powerCrystal;
    [SerializeField] private ServerComputer serverComputer;

    private bool _playerInPast;
    public static bool PlayerInPast => (SGrid.Current as FactoryGrid)._playerInPast;

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

        _playerInPast = FactoryGrid.IsInPast(Player.GetInstance().gameObject);
        FactoryTimeManager.EnableAnchorsInTime(_playerInPast);
    }

    private void Update()
    {
        bool oldValue = _playerInPast;
        _playerInPast = FactoryGrid.IsInPast(Player.GetInstance().gameObject);
        if (oldValue != _playerInPast)
        {
            playerPastChanged?.Invoke(_playerInPast);
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
