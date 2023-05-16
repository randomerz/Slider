using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitarySimContext : MonoBehaviour, MGEventListener
{
    [SerializeField] private GameObject supplyPrefab;
    [SerializeField] private MilitaryGrid grid;

    private MGSimulator _simulator;



    public bool EventFinishFlag { get; set; }

    private void Awake()
    {
        _simulator = GetComponent<MGSimulator>();
    }

    public void InitSimulation()
    {
        Debug.Log("Restarting Combat!");
        MGEventSender eSender = new MGEventSender();
        eSender.AddListener(this);
        _simulator.InitEmpty(new Vector2Int(4, 4));
    }

    public void ProcessEvent(MGEvent e)
    {
        //Pattern Matching
        //if (e is MGSpawnEvent eSpawn)
        //{
        //    Debug.Log($"Received MGSpawnEvent from MilitarySimContext.");
        //    if (eSpawn.entitySpawned == MGEntity.Supply)
        //    {
        //        GameObject instance = Instantiate(supplyPrefab, grid.GetStileAt(eSpawn.pos.x, eSpawn.pos.y).transform);
        //        instance.transform.localPosition = Vector3.zero;
        //        EventFinishFlag = true;
        //    }
        //}
    }
}
