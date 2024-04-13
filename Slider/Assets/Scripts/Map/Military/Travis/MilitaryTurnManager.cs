using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryTurnManager : Singleton<MilitaryTurnManager>
{
    [SerializeField] private MilitaryUnitCommander enemyCommander;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void EndPlayerTurn()
    {
        if (_instance != null && _instance.enemyCommander != null)  
        {
            _instance.enemyCommander.PerformTurn();
        }
    }
}
