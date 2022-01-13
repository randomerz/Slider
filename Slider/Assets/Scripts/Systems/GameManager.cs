using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    // these are just globals :L
    public static SaveSystem saveSystem { get; private set; }

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // GameObject saveSystemGO = new GameObject("Save System");
            // saveSystemGO.AddComponent<SaveSystem>();
            // saveSystem = saveSystemGO.GetComponent<SaveSystem>();
            saveSystem = new SaveSystem();

        }
    }
}
