using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DebugUIManager : MonoBehaviour
{
    private const bool debugEnabled = false;
    public GameObject debugPanel;
    public bool isDebugOpen;

    public TMP_InputField consoleText;

    private InputSettings controls;
    private static List<string> commandHistory = new List<string>();
    private static int commandIndex;

    [Header("Objects")]
    public GameObject anchorPrefab;

    private void Awake()
    {
        controls = new InputSettings();
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
        isDebugOpen = !isDebugOpen;
        Player.SetCanMove(!isDebugOpen);
        debugPanel.SetActive(isDebugOpen);

        if (isDebugOpen)
        {
            consoleText.Select();
            consoleText.ActivateInputField();
            consoleText.text = "";
            commandIndex = commandHistory.Count + 1;
        }
    }

    public void SetScene(string sceneName)
    {
        sceneName = sceneName.Trim();

        if (IsSceneInBuild(sceneName))
        {
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
                    go.gameObject.BroadcastMessage(p[0], p[1], SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public void GiveAllSliders()
    {
        for (int i = 1; i <= 9; i++)
        {
            SGrid.current.EnableStile(i);
        }
    }

    public void SpawnAnchor()
    {
        Instantiate(anchorPrefab, Player.GetPosition(), Quaternion.identity);
    }

    public void GPTC(string collectibleName)
    {
        SGrid.current.GivePlayerTheCollectible(collectibleName);
    }

    // make sure pattern is length n^2
    public void SetGrid(string pattern)
    {
        int n = (int)Mathf.Sqrt(pattern.Length);
        if (pattern.Length != n * n)
        {
            Debug.LogWarning("Input pattern was not of size n^2!");
            return;
        }

        int[,] puzzle = new int[n, n];

        for (int i = 0; i < pattern.Length; i++)
        {
            int x = i % n;
            int y = n - (i / n) - 1;
            puzzle[x, y] = int.Parse(pattern[i].ToString());
        }

        SGrid.current.SetGrid(puzzle);
    }

    public void NoClip()
    {
        Collider2D c = GameObject.Find("Player").GetComponent<Collider2D>();
        c.enabled = !c.enabled;
    }

    public void EarthQuake()
    {
        CameraShake.Shake(3f, 3f);
    }
    
}
