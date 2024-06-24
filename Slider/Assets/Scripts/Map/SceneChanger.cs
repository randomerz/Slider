using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;
    public SceneSpawns.SpawnLocation sceneSpawnName;

    public bool isSpawnPosRelative;

    public List<GameObject> deactivateOnTransition;

    public void ChangeScenes() 
    {
        SaveSystem.Current.Save();
        SceneSpawns.nextSpawn = sceneSpawnName;
        SceneSpawns.lastArea = SGrid.Current.GetArea();

        if (isSpawnPosRelative)
            SceneSpawns.relativePos = Player.GetPosition() - transform.position;

        //We want to disable menus before made to black 
        //UIManager.InvokeCloseAllMenus(true); 
        PauseManager.SetPauseState(false);

        // Start a fade to black and then load the scene once it finishes
        UIEffects.FadeToBlack(() => { StartLoadingScene(); }, 2);
    }

    // We need our loading op stored between both of our loading helper methods
    AsyncOperation sceneLoad;

    private void StartLoadingScene()
    {
        foreach(GameObject go in deactivateOnTransition)
            go.SetActive(false);
        SceneTransitionOverlayManager.ShowOverlay();

        // Stop people from opening UI!
        
      //  UIManager.InvokeCloseAllMenus(true); // In case someone had artifact open while transitioning

        /* This loads our new scene in the background. There are two components to loading a new scene:
         * 1. Actually loading it: We can do that in the background easily with LoadSceneAsync and make things smoother.
         * 2. Initializing it: All of our Awake, Start, etc methods have to run now, which takes time. To hide this, we
         *    want the screen to be fully black while this happens so the player can't see anything.
         *    
         * We do part 1 during the fade to black, then totally cover the screen (a separate overlay is needed because the first
         * will get unloaded briefly before the new UIEffects Canvas is loaded), then do part 2. Once part 2 finishes, 
         * SceneTransitionOverlayManager notices automatically and disables the overlay.
         */
        try {
            sceneLoad = SceneManager.LoadSceneAsync(sceneName);
            sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"
            StartCoroutine(IChangeScene());
        }
        catch (System.Exception e) {
            Debug.LogWarning("Scene could not be loaded! Is it properly named and added to build?");
            Debug.LogError(e);
            UIEffects.ClearScreen();
            SceneTransitionOverlayManager.HideOverlay();
        }
    }

    private IEnumerator IChangeScene()
    {
        while (sceneLoad.progress < 0.9f)
        {
            yield return null;
        }
        sceneLoad.allowSceneActivation = true; // "Okay now do it and hurry up!!"
    }
}
