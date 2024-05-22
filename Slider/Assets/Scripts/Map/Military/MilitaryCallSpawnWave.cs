using UnityEngine;

public class MilitaryCallSpawnWave : MonoBehaviour
{
    public void SpawnWave()
    {
        MilitaryWaveManager.SpawnNextWave();
    }
}