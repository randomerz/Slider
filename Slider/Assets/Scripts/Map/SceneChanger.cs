using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;
    public SceneSpawns.SpawnLocation sceneSpawnName;

    public bool isSpawnPosRelative;

    public List<GameObject> deactivateOnTransition;

    // We need our loading op stored between both of our loading helper methods
    private Coroutine coroutine;
    private AsyncOperation sceneLoad;
    private bool fadeToBlackAlmostDone;
    private bool fadeToBlackDone;

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= ResetProperties;
    }

    public void ChangeScenes() 
    {
        if (coroutine != null)
        {
            return;
        }
        
        SaveSystem.Current.Save();
        SceneSpawns.nextSpawn = sceneSpawnName;
        SceneSpawns.lastArea = SGrid.Current.GetArea();

        if (isSpawnPosRelative)
            SceneSpawns.relativePos = Player.GetPosition() - transform.position;

        //We want to disable menus before made to black 
        //UIManager.InvokeCloseAllMenus(true); 
        PauseManager.AddPauseRestriction(gameObject);
        PauseManager.SetPauseState(false);

        // Start a fade to black and then load the scene once it finishes
        UIEffects.FadeToBlackExtra(
            callbackNinety: () => { 
                fadeToBlackAlmostDone = true; 
            }, 
            callbackEnd: () => { 
                fadeToBlackDone = true; 
                SceneTransitionOverlayManager.ShowOverlay();
                foreach (GameObject go in deactivateOnTransition)
                {
                    go.SetActive(false);
                }
            }, 
            speed: 2
        );

        try {
            coroutine = StartCoroutine(StartLoadingScene());
        }
        catch (System.Exception e) {
            Debug.LogWarning("Scene could not be loaded! Is it properly named and added to build?");
            Debug.LogError(e);
            UIEffects.ClearScreen();
            UIEffects.StopCurrentCoroutine();
            SceneTransitionOverlayManager.HideOverlay();
        }
    }

    private IEnumerator StartLoadingScene()
    {

        // As it turns out, Unity's LoadSceneAsync() is not actually async :(
        // https://issuetracker.unity3d.com/issues/async-load-is-not-async

        while (!fadeToBlackAlmostDone)
        {
            yield return null;
        }

        sceneLoad = SceneManager.LoadSceneAsync(sceneName);
        sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"

        while (!fadeToBlackDone)
        {
            yield return null;
        }

        PauseManager.RemovePauseRestriction(gameObject);
        SceneManager.sceneUnloaded += ResetProperties;
        sceneLoad.allowSceneActivation = true; // "Okay now do it and hurry up!!"
    }

    public void ShowOverlayIfNotBusy()
    {
        SceneTransitionOverlayManager.ShowOverlay();
    }

    private void ResetProperties(Scene scene)
    {
        sceneName = null;
        coroutine = null;
        sceneLoad = null;
        fadeToBlackAlmostDone = false;
        fadeToBlackDone = false;
        SceneManager.sceneUnloaded -= ResetProperties;
    }
}
