using System.Collections;
using UnityEngine;

// This might not have to be a singleton. Just make sure picking up slider 1 can call the first spawnwave()
public class MilitaryWaveManager : Singleton<MilitaryWaveManager>
{
    private int nextWaveIndex;
    private readonly int[] waveSizes = new int[] { 1, 2, 3, 4, 5, 6 };
    private int nextSpawnIndex;
    private Vector2Int[] spawnPositions = new Vector2Int[] {
        new(0, -1), new(1, -1), new(2, -1), new(3, -1),
        new(0, 4),  new(1, 4),  new(2, 4),  new(3, 4),
        new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3),
        new(4, 0),  new(4, 1),  new(4, 2),  new(4, 3),
    };
    private MilitaryUnit.Type lastSpawnedType = MilitaryUnit.Type.Rock;

    private bool isSpawningWave = false;

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private MilitaryUnitCommander enemyCommander;

    public const string BEAT_ALL_ALIENS_STRING = "militaryBeatAllAliens";

    private void Awake()
    {
        InitializeSingleton();
        DoReset();
    }

    private void OnEnable()
    {
        MilitaryUnit.OnAnyUnitDeath += CheckSpawnWaveConditions;
    }

    private void OnDisable()
    {
        MilitaryUnit.OnAnyUnitDeath -= CheckSpawnWaveConditions;
    }

    public static void Reset() => _instance.DoReset();

    public void DoReset()
    {
        nextWaveIndex = 0;
        nextSpawnIndex = 0;
        lastSpawnedType = (MilitaryUnit.Type)Random.Range(0, 3);

        // Shuffle order
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            int r = Random.Range(i, spawnPositions.Length);
            (spawnPositions[i], spawnPositions[r]) = (spawnPositions[r], spawnPositions[i]);
        }
    }

    public void CheckSpawnWaveConditions(object sender, MilitaryUnit.UnitArgs e)
    {
        foreach (MilitaryUnit u in MilitaryUnit.ActiveUnits)
        {
            if (u.UnitTeam == MilitaryUnit.Team.Alien)
            {
                return;
            }
        }
        
        SpawnNextWave();
    }

    public static void SpawnNextWave()
    {
        if (_instance.isSpawningWave)
            return;
        
        _instance.isSpawningWave = true;
        CoroutineUtils.ExecuteAfterDelay(
            () => {
                _instance.SpawnWave(_instance.nextWaveIndex);
                _instance.nextWaveIndex += 1;
                _instance.isSpawningWave = false;
            }, 
            _instance, 
            1
        );
    }

    private void SpawnWave(int index)
    {
        if (index == 2)
        {
            // Let's speed things up!
            MilitaryTurnAnimator.SetBaseAnimationSpeedToMedium();
        }

        if (index >= waveSizes.Length)
        {
            CheckWinCondition();
            return;
        }

        for (int i = 0; i < waveSizes[index]; i++)
        {
            TrySpawnEnemy();
        }

        StartCoroutine(DoSpawnSoundEffects(waveSizes[index]));
    }

    private void TrySpawnEnemy()
    {
        // Quick checks -- not taken and not adjacent to ally
        for (int j = 0; j < spawnPositions.Length; j++)
        {
            Vector2Int pos = GetSpawnPosition();
            if (!IsPositionOccupied(pos) && !IsPositionAdjacentToAlly(pos))
            {
                SpawnEnemy(pos);
                return;
            }
        }

        // Try again, just not occupied!
        for (int j = 0; j < spawnPositions.Length; j++)
        {
            Vector2Int pos = GetSpawnPosition();
            if (!IsPositionOccupied(pos))
            {
                SpawnEnemy(pos);
                return;
            }
        }

        // Uh oh!
        Debug.LogError($"There were no valid locations to spawn an enemy! Spawning at (0, -1)");
        SpawnEnemy(new Vector2Int(0, -1));
    }

    private Vector2Int GetSpawnPosition()
    {
        Vector2Int pos = spawnPositions[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPositions.Length;
        return pos;
    }

    private bool IsPositionOccupied(Vector2Int position)
    {
        foreach (MilitaryUnit u in MilitaryUnit.ActiveUnits)
        {
            if (u.GridPosition == position)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPositionAdjacentToAlly(Vector2Int position)
    {
        foreach (MilitaryUnit u in MilitaryUnit.ActiveUnits)
        {
            if (u.UnitTeam == MilitaryUnit.Team.Player && (u.GridPosition - position).magnitude <= 1)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnEnemy(Vector2Int position)
    {
        // TODO: queue in animations
        GameObject go = Instantiate(enemyPrefab, MilitaryUnit.GridPositionToWorldPosition(position), Quaternion.identity, transform);
        MilitaryUnit unit = go.GetComponent<MilitaryUnit>();
        unit.InitializeNewUnit(GetNextSpawnType());
        unit.AttachedSTile = null;
        unit.GridPosition = position;
        unit.Commander = enemyCommander;
    }

    private MilitaryUnit.Type GetNextSpawnType()
    {
        // // It will be a 50% chance between either of the not just spawned types
        // lastSpawnedType = (MilitaryUnit.Type)(((int)lastSpawnedType + Random.Range(1, 3)) % 3);
        lastSpawnedType = (MilitaryUnit.Type)(((int)lastSpawnedType + 1) % 3);
        return lastSpawnedType;
    }

    private IEnumerator DoSpawnSoundEffects(int numSpawns)
    {
        AudioManager.PickSound("Portal").WithVolume(0.8f).WithPitch(0.9f).AndPlay();

        for (int i = 1; i < numSpawns; i++)
        {
            yield return new WaitForSeconds(1f);
    
            AudioManager.PickSound("Portal").WithVolume(0.5f).WithPitch(0.875f).AndPlay();
        }
    }


    private void CheckWinCondition()
    {
        // TODO: Give all Sliders if the player doesn't have them, we prob dont care if its shuffled or not
        // - mostly we care about them having tile 6 but maybe it doesnt matter and they can figure it out

        AudioManager.Play("Puzzle Complete");

        SaveSystem.Current.SetBool(BEAT_ALL_ALIENS_STRING, true);
        GiveAchievements();
    }

    private void GiveAchievements()
    {
        AchievementManager.SetAchievementStat("completedMilitary", 1);
        if(MilitaryResetChecker._instance.NumUnspawnedAlliesActive > 0)
        {
            AchievementManager.SetAchievementStat("militaryExtraAlly", 1);
        }
    }
}