using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I exist to try and fix run order because everything depends on each other :(
//L: This is disgusting, but ok.
public class SceneInitializer : MonoBehaviour
{
    public Player player;
    public SGrid sgrid;
    public UIArtifact uiArtifact;

    public static SaveProfile profileToLoad;

    private bool didInit;

    private void Awake()
    {
        if (player == null || sgrid == null || uiArtifact == null)
        {
            Debug.LogError("Scene Initializer's fields are not properly set! You might need to configure them for things to correctly load.");
        }

        if (!didInit)
            Init();
    }

    public void Init()
    {
        didInit = true;

        if (player == null)
            player = GameObject.FindObjectOfType<Player>();
        if (sgrid == null)
            sgrid = GameObject.FindObjectOfType<SGrid>();
        if (uiArtifact == null)
            uiArtifact = GameObject.FindObjectOfType<UIArtifact>();

        // DC: I LOVE DEALING WITH CIRCULAR DEPENDENCIES
        player?.InitSingleton();

        // Run inits (automatically sets singletons)
        uiArtifact?.Init();
        sgrid?.Init(); // sgrid.Load is dependent on UIArtifact singleton
        player?.Init(); // getStileUnderneath is dependent on sgrid singleton

        // Load anything else needed
        if (profileToLoad != null)
        {
            profileToLoad.Load();

            profileToLoad = null;
        }
        else if (SaveSystem.Current != null) // going to main menu
        {
            SaveSystem.Current.Load();
        }
    }
}
