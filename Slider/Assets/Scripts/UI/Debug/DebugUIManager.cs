using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class DebugUIManager : MonoBehaviour
{
    public static bool justDidSetScene;
    public static bool disableConveyers;

    public GameObject debugPanel;
    public static bool isDebugOpen;

    public TMP_InputField consoleText;

    private InputSettings controls;
    private static List<string> commandHistory = new List<string>();
    private static int commandIndex;

    public static event System.EventHandler<EventArgs> OnOpenDebug;
    public static event System.EventHandler<EventArgs> OnCloseDebug;

    [Header("Objects")]
    public GameObject anchorPrefab;
    public GameObject minecartPrefab;


    private void Awake()
    {
        controls = new InputSettings();

        controls.UI.Pause.performed += context => CloseDebug();
        controls.Debug.OpenDebug.performed += context => OnPressDebug();
        controls.Debug.CycleCommand.performed += context => CycleCommand(context.ReadValue<float>());
    }

    private void OnEnable() {
        controls.Enable();
    }
    
    private void OnDisable() {
        controls.Disable();
    }

    private void OnPressDebug() 
    {
        if (Player.GetInstance() == null || !SettingsManager.Setting<bool>(Settings.DevConsole).CurrentValue)
            return;
        
        if(!isDebugOpen)
            OpenDebug();
        else
            CloseDebug();
    }

    private void OpenDebug()
    {
        if(isDebugOpen) return;

        isDebugOpen = true;
        debugPanel.SetActive(true);
        OnOpenDebug?.Invoke(this, new EventArgs());
        PauseManager.AddPauseRestriction(owner: gameObject);
        Player.SetCanMove(false);

        consoleText.Select();
        consoleText.ActivateInputField();
        consoleText.text = "";
        commandIndex = commandHistory.Count + 1;
    }

    private void CloseDebug()
    {
        if (!isDebugOpen) return;
        
        isDebugOpen = false;
        OnCloseDebug?.Invoke(this, new EventArgs());
        Player.SetCanMove(true);
        debugPanel.SetActive(false);
        PauseManager.RemovePauseRestriction(owner: gameObject);
    }

    public void SetScene(string sceneName)
    {
        sceneName = sceneName.Trim();

        if (IsSceneInBuild(sceneName))
        {
            justDidSetScene = true;
            SaveSystem.Current.Save();
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Couldn't find scene of name " + sceneName);
        }
    }

    private bool IsSceneInBuild(string name)
    {
        // List<string> scenesInBuild = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            int lastSlash = scenePath.LastIndexOf("/");
            // scenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            // Debug.Log(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            // Debug.Log(name == scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            if (name == scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1))
                return true;
        }

        return false;
    }

    public void OnConsoleTextChange(string text)
    {
        if (text.Contains("\n"))
        {
            consoleText.text = text.Replace("\n", "");
            BroadcastMessageToAllObjects();
        }
    }

    public void CycleCommand(float value)
    {
        if (value == -1) // down key
        {
            commandIndex = Mathf.Min(commandHistory.Count, commandIndex + 1);
        }
        else if (value == 1) // up key
        {
            commandIndex = Mathf.Max(-1, commandIndex - 1);
        }

        if (commandIndex < 0 || commandIndex >= commandHistory.Count)
            consoleText.text = "";
        else
        {
            consoleText.text = commandHistory[commandIndex];
            // consoleText.caretPosition = consoleText.text.Length;
            consoleText.MoveTextEnd(false);
        }
    }

    public void BroadcastMessageToAllObjects()
    {
        string[] p = consoleText.text.Split(new char[]{' '}, 2);

        commandIndex = commandHistory.Count;

        if (p.Length == 0)
            return;

        commandHistory.Add(consoleText.text);
        commandIndex = commandHistory.Count;
        consoleText.text = "";

        if (p.Length == 1)
            Debug.Log("Called " + p[0] + "()");
        else
            Debug.Log("Called " + p[0] + "(" + p[1] + ")");


        GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject go in gos) {
            if (go && go.transform.parent == null) {
                if (p.Length == 1)
                {
                    go.gameObject.BroadcastMessage(p[0], SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    // if u need to debug something that wants floats use this >:(
                    // otherwise the errors are annoying

                    // float f;
                    // if (float.TryParse(p[1], out f))
                    // {
                    //     go.gameObject.BroadcastMessage(p[0], f, SendMessageOptions.DontRequireReceiver);
                    // }
                    // backwards compatibility some methods need strings that are numbers/ints
                    go.gameObject.BroadcastMessage(p[0], p[1], SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }


    #region Commands

    //C: modified to work for any size grid
    public void GiveAllSliders()
    {
        //Also make sure the sliders are in the right positions!
        SGrid sgrid = SGrid.Current;
        string target = sgrid.TargetGrid;
        int[,] grid = new int[sgrid.Width, sgrid.Height];

        // dc: if there's a * in the TargetGrid, then we just set them all on and are done w it lol
        // idk why the code below doesnt work for military
        if (target.Contains("*") || sgrid is MilitaryGrid)
        {
            for (int j = 1; j <= sgrid.Width * sgrid.Height; j++)
            {
                SGrid.Current.EnableStile(j);
            }
            return;
        }

        int i = 0;
        while(target.Length > 0)
        {
            char c = target[0];
            target = target.Substring(1);
            int islandId = (int) c  - '0';
            if (islandId >= 1 && islandId <= sgrid.Width * sgrid.Height)
            {
                int x = i % sgrid.Width;
                int y = sgrid.Height - 1 - i / sgrid.Width;
                grid[x, y] = islandId;
                SGrid.Current.EnableStile(islandId);
                i++;
            }
        }

        SGrid.Current.SetGrid(grid);
    }

    public void SpawnAnchor() => Instantiate(anchorPrefab, Player.GetPosition(), Quaternion.identity);
    
    public void GPTC(string collectibleName) => SGrid.Current.GivePlayerTheCollectible(collectibleName);
    
    public void Give(string collectibleName) => SGrid.Current.GivePlayerTheCollectible(collectibleName);

    public void ES(string num)
    {
        int n = int.Parse(num);
        for (int i = 1; i <= n; i++)
            SGrid.Current.GetCollectible("Slider " + i)?.DoPickUp();

        if (SGrid.Current is JungleGrid)
        {
            if (n >= 2)
            {
                SGrid.Current.GetCollectible("Slider 2 & 3").DoPickUp();
            }
        }
    }

    public void ActivateAllCollectibles(bool excludeSliders = false)
    {
        foreach (Collectible c in SGrid.Current.GetCollectibles())
            if(!excludeSliders && !c.name.Contains("Slider"))
                c.DoPickUp();
        //UIManager.CloseUI();
        PauseManager.SetPauseState(false);
    }

    //C: Gives all collectables for that area
    public void AC() => ActivateAllCollectibles();

    //C: Gives all collectables for that area, excluding Sliders
    public void ACES() => ActivateAllCollectibles(true);
    

    public void ToggleConveyers() => disableConveyers = !disableConveyers;
    
    //C: make sure pattern is the same length as the current sgrid
    public void SetGrid(string pattern)
    {
        SGrid grid = SGrid.Current;
        int width = grid.Width;
        int height = grid.Height;
        if (pattern.Length != width * height)
        {
            Debug.LogWarning("Input pattern was not the size of this SGrid!");
            return;
        }

        int[,] puzzle = new int[width, height];

        for (int i = 0; i < pattern.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            puzzle[x, y] = Converter.CharToInt(pattern[i]);
        }

        SGrid.Current.SetGrid(puzzle);
    }

    public void SetBoolTrue(string boolName) => SaveSystem.Current.SetBool(boolName, true);


    public void DebugPrintBools()
    {
        Dictionary<string, bool> bools = SaveSystem.Current.GetBoolsDictionary();
        string s = "";
        foreach (string key in bools.Keys)
        {
            s += $"{key}:{bools[key]}, \n";
        }
        Debug.Log(s);
    }

    public void NoClip()
    {
        Player p = GameObject.Find("Player").GetComponent<Player>();
        p.toggleCollision();
    }

    public void EnableScroll()
    {
        PlayerInventory.AddCollectibleFromData(new Collectible.CollectibleData("Scroll of Realigning", Area.Desert));
    }

    public void GiveBoots()
    {
        PlayerInventory.AddCollectibleFromData(new Collectible.CollectibleData("Boots", Area.Jungle));
        Player.GetInstance().UpdatePlayerSpeed();
    }
    
    public void GoToFactoryPast()
    {
        if (SGrid.Current.GetArea() != Area.Factory)
            Debug.LogWarning("GoToFactoryPast command is only valid while in the Factory area.");
        else
            FactoryTimeManager.SpawnPlayerInPast();
    }

    public void GoToFactoryPresent()
    {
        if (SGrid.Current.GetArea() != Area.Factory)
            Debug.LogWarning("GoToFactoryPresent command is only valid while in the Factory area.");
        else
            FactoryTimeManager.SpawnPlayerInPresent();
    }

    public void EarthQuake() => CameraShake.Shake(3f, 3f);
    
    #endregion
}
