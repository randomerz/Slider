using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI instance { get; private set; }
    private bool didInit;

    public class GameUICanvas
    {
        public GameUICanvas(GameObject prefab)
        {
            this.prefab = prefab;
        }

        //public System.Type myType;
        public GameObject prefab;
        public GameObject singleton;

        public void Singlify()
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("GameUI"))
            {
                Debug.Log("looking at: " + go);
                if (go.name != prefab.name)
                    continue;
                // if there's one in the scene, may as well take it
                if (singleton == null)
                {
                    singleton = go;
                    DontDestroyOnLoad(singleton);
                }
                // destroy all other objects
                else if (go.GetInstanceID() != singleton.GetInstanceID())
                {
                    Debug.Log("Destroying! " + go);
                    go.SetActive(false); // this is so we don't run it's awake (hopefully)
                    Destroy(go);
                }
            }
            
            // if there wasn't one in the scene...
            if (singleton == null)
            {
                singleton = Instantiate(prefab);
                DontDestroyOnLoad(singleton);
            }
        }
    }

    public GameObject UICanvasPrefab;
    public GameObject UIEffectsPrefab;
    public GameObject DebugCanvasPrefab;
    public GameObject SceneTransitionOverlayPrefab;

    private List<GameUICanvas> uiCanvases = new List<GameUICanvas>();

    private void Awake()
    {
        Init();
        //Singlify();

        Debug.Log("GameUI Awake");
    }

    public void Init()
    {
        if (didInit)
            return;
        didInit = true;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            uiCanvases.Add(new GameUICanvas(UICanvasPrefab));
            uiCanvases.Add(new GameUICanvas(UIEffectsPrefab));
            uiCanvases.Add(new GameUICanvas(DebugCanvasPrefab));
            uiCanvases.Add(new GameUICanvas(SceneTransitionOverlayPrefab));
        }
        
        instance.Singlify();
    }

    public void Singlify()
    {
        Debug.Log("Singlifying");
        foreach (GameUICanvas c in uiCanvases)
        {
            c.Singlify();
        }
    }

    public void SetCanvasesActive(bool value)
    {
        foreach (GameUICanvas c in uiCanvases)
        {
            c.singleton.SetActive(value);
        }
    }
}
