using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEffects : Singleton<UIEffects>
{
    public GameObject blackPanel;
    public CanvasGroup blackPanelCanvasGroup;
    public float fadeDuration = 1;

    public GameObject whitePanel;
    public CanvasGroup whitePanelCanvasGroup;
    public float flashDuration = 1;

    public GameObject screenshotPanel;
    public CanvasGroup screenshotCanvasGroup;
    public Image screenshotImage;

    public UISpotlightEffect uISpotlightEffect;

    public PixelizeFeature pixelizeFeature;

    //private static UIEffects _instance;

    // Holds the last flashing/black fade coroutine called so we can end it when starting a new one
    private static Coroutine previousCoroutine;
    private Texture2D screenshot;

    public enum ScreenshotEffectType
    {
        PORTAL,
        MIRAGE
    };

    private void Awake()
    {
        //_instance = this;
        InitializeSingleton(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneChange;
    }

    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals(NewSavePanelManager.CUTSCENE_SCENE_NAME))
        {
            _instance.blackPanel.SetActive(false);
        }
    }

    public static void FadeFromBlack(System.Action callback=null, float speed=1, float alpha = 1f)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.blackPanel, _instance.blackPanelCanvasGroup, alpha, 0, callback, speed));
    }

    public static void FadeToBlack(System.Action callback=null, float speed=1, bool disableAtEnd = true, float alpha = 1f)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.blackPanel, _instance.blackPanelCanvasGroup, 0, alpha, callback, speed, disableAtEnd));
    }

    public static void FadeFromWhite(System.Action callback=null, float speed=1)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.whitePanel, _instance.whitePanelCanvasGroup, 0.9f, 0, callback, speed));
    }

    public static void FadeToWhite(System.Action callback=null, float speed=1, bool disableAtEnd = true, float alpha = 1, bool useUnscaledTime = false)
    {
        StartEffectCoroutine(_instance.FadeCoroutine(_instance.whitePanel, _instance.whitePanelCanvasGroup, 0, alpha, callback, speed, disableAtEnd, useUnscaledTime));
    }

    public static void FlashWhite(System.Action callbackMiddle=null, System.Action callbackEnd=null, float speed=1, bool useUnscaledTime = false)
    {
        StartEffectCoroutine(_instance.FlashCoroutine(callbackMiddle, callbackEnd, speed, useUnscaledTime));
    }

    public static void FadeFromScreenshot(ScreenshotEffectType type, System.Action screenshotCallback = null, System.Action callbackEnd = null, float speed = 1)
    {
        StartEffectCoroutine(_instance.ScreenshotCoroutine(type, screenshotCallback, callbackEnd, speed), false);
    }

    public static void Pixelize(System.Action callbackMiddle=null, System.Action callbackEnd=null, float speed=1)
    {
        StartEffectCoroutine(_instance.PixelizeCoroutine(callbackMiddle, callbackEnd, speed));
    }

    public static void ClearScreen()
    {
        _instance.blackPanel.SetActive(false);
        _instance.blackPanelCanvasGroup.alpha = 0;
        _instance.whitePanel.SetActive(false);
        _instance.whitePanelCanvasGroup.alpha = 0;
        _instance.screenshotPanel.SetActive(false);
        _instance.screenshotCanvasGroup.alpha = 0;
    }

    private static void StartEffectCoroutine(IEnumerator coroutine, bool stopable = true)
    {
        if (previousCoroutine != null)
        {
            _instance.StopCoroutine(previousCoroutine);
            ClearScreen();
        }
        var c = _instance.StartCoroutine(coroutine);
        previousCoroutine = stopable ? c : null;
    }


    private IEnumerator FadeCoroutine(GameObject gameObject, CanvasGroup group, float startAlpha, float endAlpha, System.Action callback=null, float speed=1, bool disableAtEnd = true, bool useUnscaledTime = false)
    {
        float t = 0;
        gameObject.SetActive(true);
        group.alpha = startAlpha;

        while (t < fadeDuration)
        {
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);

            yield return null;
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime * speed);
        }

        group.alpha = endAlpha;
        if(disableAtEnd)
            gameObject.SetActive(false);

        callback?.Invoke();
    }

    private IEnumerator FlashCoroutine(System.Action callbackMiddle=null, System.Action callbackEnd=null, float speed=1, bool useUnscaledTime = false)
    {
        float t = 0;
        whitePanel.SetActive(true);
        whitePanelCanvasGroup.alpha = 0;

        while (t < flashDuration / 2)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime * speed);
        }

        callbackMiddle?.Invoke();

        while (t >= 0)
        {
            whitePanelCanvasGroup.alpha = Mathf.Lerp(0, 1, t / (flashDuration / 2));

            yield return null;
            t -= (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime * speed);
        }

        whitePanelCanvasGroup.alpha = 0;
        whitePanel.SetActive(false);
        
        callbackEnd?.Invoke();
    }

    public static void TakeScreenshot()
    {
        _instance.screenshot =  ScreenCapture.CaptureScreenshotAsTexture();
    }

    private IEnumerator ScreenshotCoroutine(ScreenshotEffectType type, System.Action screenshotCallback = null, System.Action callbackEnd = null, float speed = 1)
    {
        yield return new WaitForEndOfFrame();
        if(screenshot == null)
            screenshot =  ScreenCapture.CaptureScreenshotAsTexture();
        Sprite sprite = Sprite.Create(screenshot, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0, 0));
        screenshotImage.sprite = sprite;
        screenshotPanel.SetActive(true);
        screenshotCanvasGroup.alpha = 0.5f;
        screenshotPanel.transform.localScale = Vector3.one;
        screenshotPanel.transform.localRotation = Quaternion.identity;
        screenshotCallback?.Invoke();

        float t = 0;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(0.5f, 0, t / fadeDuration);
            screenshotCanvasGroup.alpha = alpha;
            switch (type)
            {
                case ScreenshotEffectType.PORTAL:
                    screenshotPanel.transform.localScale = (2 - (alpha * 2)) * Vector3.one;
                    screenshotPanel.transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, t * 2));
                    break;
                case ScreenshotEffectType.MIRAGE:
                    screenshotPanel.transform.localScale = Mathf.Lerp(1, 1.1f, (t/ fadeDuration)) * Vector3.one;
                    break;
            }
            yield return null;
            t += (Time.deltaTime * speed);
        }
        screenshotPanel.SetActive(false);
        screenshot = null;
        callbackEnd?.Invoke();
    }

    private IEnumerator PixelizeCoroutine(System.Action callbackMiddle = null, System.Action callbackEnd = null, float speed = 1)
    {   
        int maxRes = 180;
        int minRes = 1;

        float t = 0; 
        pixelizeFeature.settings.enabled = true;

        while (t < flashDuration / 2)
        {
            pixelizeFeature.settings.screenHeight = (int)Mathf.Lerp(maxRes, minRes, t / (flashDuration / 2));

            yield return null;
            t += (Time.deltaTime * speed);
        }

        callbackMiddle?.Invoke();

        while (t >= 0)
        {
            pixelizeFeature.settings.screenHeight = (int)Mathf.Lerp(maxRes, minRes, t / (flashDuration / 2));

            yield return null;
            t -= (Time.deltaTime * speed);
        }
        callbackEnd?.Invoke();
        pixelizeFeature.settings.enabled = false;
    }

    public static void DisablePixel()
    {
        if(_instance == null) return;
        _instance.pixelizeFeature.settings.enabled = false;
    }

    public static void StartSpotlight(Vector2 positionPixel, float radiusPixel, float duration=2, System.Action onStart=null, System.Action onFinish=null)
    {
        _instance.uISpotlightEffect.StartSpotlight(positionPixel, radiusPixel, duration, onStart, onFinish);
    }

    public static void EndSpotlightEarly() => _instance.uISpotlightEffect.EndSpotlightEarly();
}
