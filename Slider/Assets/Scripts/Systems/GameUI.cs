using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI instance { get; private set; }
    private bool didInit;
    public bool isMenuScene;
    public List<string> menuScenes;

    [System.Serializable]
    public class GameUICanvas
    {
        //public System.Type myType;
        public GameObject prefab;
        public bool activeInMenuScenes;
        public bool containsLocalizable;
        private GameObject singleton;

        public void Singlify()
        {
            if (containsLocalizable)
            {
                foreach (var go in GameObject.FindGameObjectsWithTag("GameUI").Where(go => go.name.Equals(prefab.name)))
                {
                    Destroy(go);
                }
                
                singleton = Instantiate(prefab);
                singleton.name = prefab.name;
                DontDestroyOnLoad(singleton);
            }
            
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("GameUI"))
            {
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
                    //if(go.GetComponent<GameUI>() != null)
                    //    singleton.GetComponent<GameUI>().isMenuScene = go.GetComponent<GameUI>().isMenuScene;
                    go.SetActive(false); // this is so we don't run it's awake (hopefully)
                    Destroy(go);
                }
            }
            
            // if there wasn't one in the scene...
            if (singleton == null)
            {
                singleton = Instantiate(prefab);
                singleton.name = prefab.name;
                DontDestroyOnLoad(singleton);
            }
        }

        public void SetActive(bool value)
        {
            if (activeInMenuScenes) // in main menu scenes, leave this on
                return;
            singleton.SetActive(value);
        }
    }

    public GameUICanvas UICanvasPrefab;
    public GameUICanvas UIEffectsPrefab;
    public GameUICanvas DebugCanvasPrefab;
    public GameUICanvas SceneTransitionOverlayPrefab;

    private List<GameUICanvas> uiCanvases = new List<GameUICanvas>();

    private void Awake()
    {
        Init();
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

            uiCanvases.Add(UICanvasPrefab);
            uiCanvases.Add(UIEffectsPrefab);
            uiCanvases.Add(DebugCanvasPrefab);
            uiCanvases.Add(SceneTransitionOverlayPrefab);
        }
        
        instance.Singlify();

       // instance.SetCanvasesActive(!isMenuScene); // if its a menu scene, lets turn off the canvases
    }

    private void OnEnable() {
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    private void OnDisable() {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private void OnSceneChange (Scene curr, Scene next)
    {
        if(menuScenes.Contains(next.name))
            isMenuScene = true;
        else
            isMenuScene = false;
    }

    public void Singlify()
    {
        //Debug.Log("Singlifying");
        foreach (GameUICanvas c in uiCanvases)
        {
            c.Singlify();
        }
    }

    public void SetCanvasesActive(bool value)
    {
        foreach (GameUICanvas c in uiCanvases)
        {
            c.SetActive(value);
        }
    }
}
