using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryGrid : SGrid
{
    // [Header("FactoryGrid")]
    // [SerializeField] private PowerCrystal powerCrystal;
    public FactoryMusicController factoryMusicController;

    public static bool PlayerInPast => IsInPast(Player.GetInstance().gameObject);
    private bool _lastPlayerInPast = false;

    public static event System.EventHandler PlayerChangedTime;

    public override void Init() {
        InitArea(Area.Factory);
        base.Init();
    }

    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Factory");

        if (PlayerInPast)
        {
            Player.SetIsInHouse(true);
        }
    }

    private void Update()
    {
        if (PlayerInPast != _lastPlayerInPast)
        {
            _lastPlayerInPast = PlayerInPast;
            PlayerChangedTime?.Invoke(this, null);
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

    protected override void CheckForCompletionOnSetGrid()
    {
        CheckForFactoryCompletion();
    }

    public void CheckForFactoryCompletion() {
        if(CheckGrid.contains(GetGridString(), "851_769_243")) {
            StartCoroutine(ShowButtonAndMapCompletions());
            AchievementManager.SetAchievementStat("completedFactory", 1);
        }
    }

}
