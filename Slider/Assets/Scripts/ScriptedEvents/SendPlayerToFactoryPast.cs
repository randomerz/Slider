using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendPlayerToFactoryPast : MonoBehaviour
{
    [SerializeField] private PlayerPositionChanger ppChanger;
    [SerializeField] private GameObject[] pastTileMaps;

    private Conveyor[] conveyors;

    private void Awake()
    {
        conveyors = GameObject.FindObjectsOfType<Conveyor>();
    }
    public void StartSendToPastEvent()
    {
        //Do Cool Cutscene Stuff (see Trello design card)

        foreach (Conveyor conv in conveyors)
        {
            conv.ConveyorEnabled = false;
        }

        foreach (GameObject go in pastTileMaps)
        {
            go.SetActive(true);
        }

        ppChanger.UPPRelativePlayer();
    }
}
