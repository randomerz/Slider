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

    public CanvasGroup canvasGroupReunited;
    public CanvasGroup canvasGroupWith;
    public CanvasGroup canvasGroupBoomo;
    public CanvasGroup timeCanvasGroup;

    // private const float MAXIMUM_TIME_DISPLAY_SECONDS = 3600 * 3;
    private const float MAXIMUM_SLOW_SPEEDRUN_ACHIEVEMENT_TIME_SECONDS = 3600 * 2;
    private const float MAXIMUM_FAST_SPEEDRUN_ACHIEVEMENT_TIME_SECONDS = 3600;
    private const float PARALLAX_ANIMATION_DURATION = 5;
    private const float FADE_ANIMATION_DURATION = 1;
    private const float FADE_INBETWEEN_DURATION = 0.75f;
    private const string CREDITS_SCENE = "Credits";

    private System.IDisposable listener;
    private AsyncOperation sceneLoad;

    private float time;

    private void OnDisable() 
    {
        listener?.Dispose();
    }

    public void Start()
    {
        time = SaveSystem.Current.GetPlayTimeInSeconds();
        UpdateTexts();
        StartCoroutine(AnimateEndScene());
    }

    private void GiveAchievements()
    {
        AchievementManager.SetAchievementStat("savedCat", false, 1);
        if (SaveSystem.Current != null && time < MAXIMUM_SLOW_SPEEDRUN_ACHIEVEMENT_TIME_SECONDS)
        {
            AchievementManager.SetAchievementStat("completedGame2Hours", true, 1);
        }
        if (SaveSystem.Current != null && time < MAXIMUM_FAST_SPEEDRUN_ACHIEVEMENT_TIME_SECONDS)
        {
            AchievementManager.SetAchievementStat("completedGame1Hour", true, 1);
        }
    }

    public void UpdateTexts()
    {
        if (SaveSystem.Current == null)
        {
            Debug.LogError("Couldn't load save profile.");
            return;
        }

        nameText.SetText($"{SaveSystem.Current.GetProfileName()}!");
        TimeSpan ts = TimeSpan.FromSeconds(time);
        timeText.SetText(string.Format(
            "{0:D2}:{1:D2}:{2:D2}:{3:D3}",
            ts.Hours,
            ts.Minutes,
            ts.Seconds,
            ts.Milliseconds
        ));

        // bool enableTimeText = time <= MAXIMUM_TIME_DISPLAY_SECONDS;
        bool enableTimeText = true;
        timeText.gameObject.SetActive(enableTimeText);
    }

    /// <summary>
    /// Main method for animating the scene when it starts, called in Start()
    /// </summary>
    private IEnumerator AnimateEndScene()
    {
        UIEffects.FadeFromBlack(speed: 0.5f);
        canvasGroupReunited.alpha = 0;
        canvasGroupWith.alpha = 0;
        canvasGroupBoomo.alpha = 0;
        timeCanvasGroup.alpha = 0;

        StartCoroutine(ParalaxAnimation());

        yield return new WaitForSeconds(PARALLAX_ANIMATION_DURATION - 0.25f - 1.5f);

        StartCoroutine(FadeAnimation(canvasGroupReunited));

        yield return new WaitForSeconds(FADE_INBETWEEN_DURATION);

        StartCoroutine(FadeAnimation(canvasGroupWith));

        yield return new WaitForSeconds(FADE_INBETWEEN_DURATION);

        StartCoroutine(FadeAnimation(canvasGroupBoomo));

        yield return new WaitForSeconds(2 * FADE_INBETWEEN_DURATION);

        StartCoroutine(FadeAnimation(timeCanvasGroup));

        yield return new WaitForSeconds(2 * FADE_INBETWEEN_DURATION);

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
        GoToCredits();
    }

    private void GoToCredits()
    {
        SaveSystem.SetCurrentProfile(-1); 
        sceneLoad = SceneManager.LoadSceneAsync(CREDITS_SCENE);
        sceneLoad.allowSceneActivation = false;

        GiveAchievements();

        UIEffects.FadeToBlack(() => {
            sceneLoad.allowSceneActivation = true;
        }, disableAtEnd: false);
    }
}
