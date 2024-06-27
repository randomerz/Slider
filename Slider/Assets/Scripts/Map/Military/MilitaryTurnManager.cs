using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryTurnManager : Singleton<MilitaryTurnManager>
{
    public static System.EventHandler<System.EventArgs> OnPlayerEndTurn;

    [SerializeField] private MilitaryUnitCommander enemyCommander;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void EndPlayerTurn()
    {
        OnPlayerEndTurn?.Invoke(_instance, new System.EventArgs());

        if (_instance != null && _instance.enemyCommander != null)  
        {
            _instance.enemyCommander.PerformTurn();
        }
    }
}
