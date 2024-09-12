using UnityEngine;

public class MilitaryResetChecker : Singleton<MilitaryResetChecker>
{
    private int numSliderCollectiblesActive = 0;
    private int numUnspawnedAlliesActive = 0;

    private int numSpawnedAllies = 0;
    public int NumSpawnedAllies => numSpawnedAllies;
    public const int TOTAL_POSSIBLE_ALLIES = 8;

    private bool didInit = false;
    
    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        if (didInit)
            return;
        didInit = true;
        
        InitializeSingleton();
    }

    private void OnEnable()
    {
        MilitaryUnit.OnAnyUnitDeath += OnUnitDeath;
    }

    private void OnDisable()
    {
        MilitaryUnit.OnAnyUnitDeath -= OnUnitDeath;
    }

    public static void ResetCounters()
    {
        _instance.numSliderCollectiblesActive = 0;
        _instance.numUnspawnedAlliesActive = 0;
        _instance.numSpawnedAllies = 0;
    }

    public void OnUnitDeath(object sender, MilitaryUnit.UnitArgs e)
    {
        if (e.unit.UnitTeam == MilitaryUnit.Team.Player)
        {
            DoResetCheck();
        }
    }

    // Check on ally death
    public void DoResetCheck()
    {
        // Debug.Log($"Reset conditions: {AreAnyAlliesAlive()}, {numSliderCollectiblesActive}, {numUnspawnedAlliesActive}");
        if (AreResetConditionsMet())
        {
            // Double check in case of weird ordering like fighting w last unit -> spawn reward
            CoroutineUtils.ExecuteAfterDelay(() => {
                if (AreResetConditionsMet())
                {
                    // Ideally use the glitch post-process effect from the fezziwig time loop
                    // Debug.Log("Do an auto Reset!!!!");
                    (MilitaryGrid.Current as MilitaryGrid).RestartSimulation(0.25f);
                }
            }, this, 2);
        }
    }

    private bool AreResetConditionsMet()
    {
        bool didWin = SaveSystem.Current.GetBool(MilitaryWaveManager.BEAT_ALL_ALIENS_STRING);
        return !didWin && !(AreAnyAlliesAlive() || numSliderCollectiblesActive > 0 || numUnspawnedAlliesActive > 0);
    }

    private bool AreAnyAlliesAlive()
    {
        foreach (MilitaryUnit unit in MilitaryUnit.ActiveUnits)
        {
            if (unit.UnitTeam == MilitaryUnit.Team.Player)
            {
                return true;
            }
        }
        return false;
    }

    public static void IncrementCollectible()
    {
        _instance.numSliderCollectiblesActive += 1;
    }

    public static void DecrementCollectible()
    {
        _instance.numSliderCollectiblesActive -= 1;
        _instance.DoResetCheck();
    }

    public static void IncrementUnspawnedAlly()
    {
        _instance.numUnspawnedAlliesActive += 1;
    }

    public static void DecrementUnspawnedAlly()
    {
        if (_instance != null)
        {
            _instance.numUnspawnedAlliesActive -= 1;
        }
    }

    public static void IncrementSpawnedAlly()
    {
        if (_instance != null)
        {
            _instance.numSpawnedAllies += 1;
        }
    }
}