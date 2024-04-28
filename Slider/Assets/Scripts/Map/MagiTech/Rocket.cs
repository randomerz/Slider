using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    public CameraDolly rocketCameraDolly;
    public GameObject rocket;
    public SpriteRenderer rocketSpriteRenderer;
    public Transform rocketStart;
    public Transform rocketEnd;
    public AnimationCurve rocketCurve;
    private bool isPlaying;
    private float rocketDuration = 8.0f;
    private AsyncOperation sceneLoad;
    private const string END_OF_GAME_SCENE = "EndOfGame";

    public void StartRocketCutscene()
    {
        if(isPlaying) return;
        StartCoroutine(RocketCutscene());
    }

    private IEnumerator RocketCutscene()
    {
        sceneLoad = SceneManager.LoadSceneAsync(END_OF_GAME_SCENE);
        sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"

        rocketSpriteRenderer.sortingOrder = 25; // set to entity 25
        UIEffects.FadeToBlack(disableAtEnd: false);
        yield return new WaitForSeconds(1.1f);
        Player.GetSpriteRenderer().enabled = false;
        List<Vector2> shakeData = new()
        {
            Vector2.zero,
            new(2.5f, 1f),
            new(5f, 1f),
            new(9f, 0f)
        };
        CameraShake.ShakeCustom(shakeData);
        // CameraShake.Shake(rocketDuration, 1);
        rocketCameraDolly.OnRollercoasterEnd += OnSkipCutscene;
        rocketCameraDolly.StartTrack(true);

        float t = 0f;
        while (t < rocketDuration)
        {
            float y = rocketCurve.Evaluate(t / rocketDuration);
            Vector3 pos = new Vector3(rocketStart.position.x,
                                      Mathf.Lerp(rocketStart.position.y, rocketEnd.position.y, y));
            rocket.transform.position = pos;
            t += Time.deltaTime;
            
            yield return null;
        }

        LoadEndOfGameScene();
    }

    private void LoadEndOfGameScene()
    {
        SaveSystem.SaveGame("Ending Game");
        SaveSystem.Current.SetCompletionStatus(true);
        // Undo lazy singletons
        if(Player.GetInstance() != null)
            Player.GetInstance().ResetInventory();
        rocketCameraDolly.OnRollercoasterEnd -= OnSkipCutscene;
        sceneLoad.allowSceneActivation = true;
    }

    private void OnSkipCutscene(object sender, System.EventArgs e)
    {
        StopAllCoroutines();
        LoadEndOfGameScene();
    }
}
