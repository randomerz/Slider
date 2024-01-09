using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class EndOfGameManager : MonoBehaviour
{
    [System.Serializable]
    public struct ParallaxPlane
    {
        public GameObject gameObject;
        public float paralaxFactor;
    }
    
    public UIBigText nameText;
    public UIBigText timeText;
    public List<ParallaxPlane> parallaxPlanes;
    public Transform parallaxAnimationStart;
    public Transform parallaxAnimationEnd;
    public AnimationCurve parallaxAnimationCurve;

    public CanvasGroup canvasGroup;
    public CanvasGroup timeCanvasGroup;

    private const float MAXIMUM_SPEEDRUN_TIME_SECONDS = 7200;
    private const float PARALLAX_ANIMATION_DURATION = 5;
    private const float FADE_ANIMATION_DURATION = 1;
    private const string MAIN_MENU_SCENE = "MainMenu";

    private System.IDisposable listener;
    private AsyncOperation sceneLoad;

    private void OnDisable() 
    {
        listener?.Dispose();
    }

    public void Start()
    {
        UpdateTexts();
        StartCoroutine(AnimateEndScene());
    }

    public void UpdateTexts()
    {
        if (SaveSystem.Current == null)
        {
            Debug.LogError("Couldn't load save profile.");
            return;
        }

        nameText.SetText($"{SaveSystem.Current.GetProfileName()}!");
        TimeSpan ts = TimeSpan.FromSeconds(SaveSystem.Current.GetPlayTimeInSeconds());
        timeText.SetText(string.Format(
            "{0:D2}:{1:D2}:{2:D2}:{3:D3}",
            ts.Hours,
            ts.Minutes,
            ts.Seconds,
            ts.Milliseconds
        ));

        bool enableTimeText = SaveSystem.Current.GetPlayTimeInSeconds() <= MAXIMUM_SPEEDRUN_TIME_SECONDS;
        timeText.gameObject.SetActive(enableTimeText);
    }

    /// <summary>
    /// Main method for animating the scene when it starts, called in Start()
    /// </summary>
    private IEnumerator AnimateEndScene()
    {
        UIEffects.FadeFromBlack();
        canvasGroup.alpha = 0;
        timeCanvasGroup.alpha = 0;

        StartCoroutine(ParalaxAnimation());

        yield return new WaitForSeconds(PARALLAX_ANIMATION_DURATION - 0.25f);

        StartCoroutine(FadeAnimation(canvasGroup));

        yield return new WaitForSeconds(FADE_ANIMATION_DURATION + 0.5f);

        StartCoroutine(FadeAnimation(timeCanvasGroup));

        yield return new WaitForSeconds(FADE_ANIMATION_DURATION + 0.5f);

        EnableOnAnyButtonPress();
    }

    private IEnumerator ParalaxAnimation()
    {
        float t = 0;

        while (t < PARALLAX_ANIMATION_DURATION)
        {
            float x = parallaxAnimationCurve.Evaluate(t / PARALLAX_ANIMATION_DURATION);
            UpdateParallax(Vector3.LerpUnclamped(parallaxAnimationStart.position, parallaxAnimationEnd.position, x));

            yield return null;
            t += Time.deltaTime;
        }

        UpdateParallax(parallaxAnimationEnd.position);

        StartCoroutine(ParalaxLoop());
    }

    private IEnumerator ParalaxLoop()
    {
        float startTime = Time.time;

        while (true)
        {
            float x = Mathf.Sin(Time.time - startTime);
            UpdateParallax(parallaxAnimationEnd.position + x * Vector3.up);

            yield return null;
        }
    }

    private void UpdateParallax(Vector3 pos)
    {
        Vector3 offset = pos - parallaxAnimationEnd.position;
        foreach (ParallaxPlane pp in parallaxPlanes)
        {
            pp.gameObject.transform.position = parallaxAnimationEnd.position + offset * pp.paralaxFactor;
        }
    }

    private IEnumerator FadeAnimation(CanvasGroup group)
    {
        float t = 0;

        while (t < FADE_ANIMATION_DURATION)
        {
            float x = t / FADE_ANIMATION_DURATION;
            group.alpha = x;

            yield return null;
            t += Time.deltaTime;
        }

        group.alpha = 1;
    }

    private void EnableOnAnyButtonPress()
    {
        listener = InputSystem.onAnyButtonPress.Call(ctrl => OnAnyButtonPress());
    }

    private void OnAnyButtonPress() 
    {
        listener.Dispose();
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        sceneLoad = SceneManager.LoadSceneAsync(MAIN_MENU_SCENE);
        sceneLoad.allowSceneActivation = false; // "Don't initialize the new scene, just have it ready"

        UIEffects.FadeToBlack(() => {
            sceneLoad.allowSceneActivation = true;
        }, disableAtEnd: false);
    }
}
