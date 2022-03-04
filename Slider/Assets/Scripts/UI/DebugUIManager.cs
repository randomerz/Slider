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

    public TMP_InputField sceneChangeText;

    private InputSettings controls;

    private void Awake()
    {

        controls = new InputSettings();
        controls.Debug.OpenDebug.performed += context => OnPressDebug();
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
        debugPanel.SetActive(isDebugOpen);
    }

    public void TrySetScene()
    {
        string sceneText = sceneChangeText.text.Trim();

        if (IsSceneInBuild(sceneText))
        {
            SceneManager.LoadScene(sceneText);
        }
        else
        {
            Debug.LogWarning("Couldn't find scene of name " + sceneText);
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
}
