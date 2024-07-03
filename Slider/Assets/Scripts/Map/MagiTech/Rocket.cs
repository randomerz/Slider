using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    public GameObject rocket;
    public SpriteRenderer rocketSpriteRenderer;
    public Transform rocketStart;
    public Transform rocketEnd;
    public AnimationCurve rocketCurve;

    public CameraDolly rocketCameraDolly;
    public List<ParticleSystem> rocketParticles;

    private bool isPlaying;
    private const float ROCKET_DURATION = 8.0f;

    private AsyncOperation sceneLoad;
    private const string END_OF_GAME_SCENE = "EndOfGame";

    public void StartRocketCutscene()
    {
        if (isPlaying) 
            return;

        StartCoroutine(RocketCutscene());
    }

    private IEnumerator RocketCutscene()
    {
        isPlaying = true;
        
        sceneLoad = SceneManager.LoadSceneAsync(END_OF_GAME_SCENE);
        sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"

        rocketSpriteRenderer.sortingOrder = 25; // set to entity 25
        UIEffects.FadeToBlack(disableAtEnd: false);

        foreach (ParticleSystem ps in rocketParticles)
        {
            ps.Play();
        }

        // Really jank way of turning down music
        AudioManager.DampenMusic(this, 0, 6);
        CoroutineUtils.ExecuteAfterDelay(() => {
            AudioManager.StopDampen(this);
            AudioManager.StopMusic("MagiTech");
            AudioManager.Play("MagiTech Blast Off");
        }, this, 1);

        yield return new WaitForSeconds(3f);

        AudioManager.Play("Rumble Increase 8s");

        Player.GetSpriteRenderer().enabled = false;
        List<Vector2> shakeData = new()
        {
            Vector2.zero,
            new(2.5f, 0.5f),
            new(5f, 0.5f),
            new(9f, 0f)
        };
        CameraShake.ShakeCustom(shakeData);
        
        rocketCameraDolly.OnRollercoasterEnd += OnSkipCutscene;
        rocketCameraDolly.StartTrack(true);

        CoroutineUtils.ExecuteEachFrame(
            (x) => {
                rocket.transform.position = Vector3.Lerp(rocketStart.position, rocketEnd.position, x);
            },
            () => {},
            this,
            ROCKET_DURATION,
            rocketCurve
        );

        CoroutineUtils.ExecuteAfterDelay(() => {
            UIEffects.FadeToBlack(() => LoadEndOfGameScene(), 0.5f);
        }, this, ROCKET_DURATION - 2);
    }

    private void LoadEndOfGameScene()
    {
        SceneTransitionOverlayManager.ShowOverlay();

        SaveSystem.SaveGame("Ending Game");
        SaveSystem.Current.SetCompletionStatus(true);

        // Undo lazy singletons
        if (Player.GetInstance() != null)
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
