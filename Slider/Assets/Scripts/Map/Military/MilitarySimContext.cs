using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitarySimContext : MonoBehaviour
{
    [SerializeField] private Vector2Int boardDims;
    [SerializeField] private List<MGUnitData.Data> possibleUnits;
    //[SerializeField] private GameObject supplyPrefab;

    private MGSimulator _simulator;

    public bool EventFinishFlag { get; set; }

    private void Awake()
    {
        AudioManager.PlayMusic("MilitarySim");
        _simulator = new MGSimulator();
        _simulator.Init(boardDims);
    }

    private void Start()
    {
        _simulator.PopulateRandom(possibleUnits);
        _simulator.PrintSimulatorState();
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
