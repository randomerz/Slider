using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitarySimContext : MonoBehaviour
{
    [SerializeField] private Vector2Int boardDims;
    [SerializeField] private List<MGUnitData> possibleUnits;
    //[SerializeField] private GameObject supplyPrefab;

    private MGSimulator _simulator;
    public MGSimulator Simulator => _simulator;

    public bool EventFinishFlag { get; set; }

    private void Awake()
    {
        AudioManager.PlayMusic("MilitarySim");
        _simulator = new MGSimulator();
        _simulator.Init(boardDims);
    }

    private IEnumerator Start()
    {
        yield return null;
        MGUnit unit = _simulator.SpawnUnit(0, 0, new MGUnitData(MGJob.Rock, MGSide.Ally));
        MGUnit enemy = _simulator.SpawnUnit(1, 0, new MGUnitData(MGJob.Scissors, MGSide.Enemy));
        yield return new WaitForSeconds(1f);
        _simulator.MoveUnit(unit, 1, 0);
    }

    //public void ProcessEvent(MGEvent e)
    //{
    //    //Pattern Matching
    //    if (e is MGSpawnEvent eSpawn)
    //    {
    //        Debug.Log($"Received MGSpawnEvent from MilitarySimContext.");
    //        if (eSpawn.entitySpawned == MGEntity.Supply)
    //        {
    //            GameObject instance = Instantiate(supplyPrefab, grid.GetStileAt(eSpawn.pos.x, eSpawn.pos.y).transform);
    //            instance.transform.localPosition = Vector3.zero;
    //            EventFinishFlag = true;
    //        }
    //    }
    //}
}
