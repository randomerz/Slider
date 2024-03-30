using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ConventionModeManager : MonoBehaviour
{
    public static ConventionModeManager instance { get; private set; }

    public float timeAllowedInactive = 90;
    private float timeUntilInactive;
    private System.IDisposable listener;
    
    public float restartDuration = 10;
    private bool isInRestartCountdown;
    private float timeUntilRestart;
    
    private float backgroundPanelAlphaStart;
    private float backgroundPanelAlphaEnd = 1;
    private Coroutine animateAlphaCoroutine;

    [Header("References")]
    public CanvasGroup canvasGroup;
    public Image backgroundPanel;
    public QuitHandler myQuitHandler;

    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        backgroundPanelAlphaStart = backgroundPanel.color.a;
        OnAnyButtonPress();
    }
    
    void Start()
    {
        EnableOnAnyButtonPress();
    }
    
    void Update()
    {
        if (GameUI.instance.isMenuScene)
        {
            return;
        }

        if (isInRestartCountdown)
        {
            if (timeUntilRestart <= 0)
            {
                Restart();
            }

            timeUntilRestart -= Time.deltaTime;
        }
        else if (timeUntilInactive <= 0)
        {
            SetIsInRestartCountdown(true);
        }

        timeUntilInactive -= Time.deltaTime;
    }

    private void EnableOnAnyButtonPress()
    {
        listener = InputSystem.onAnyButtonPress.Call(ctrl => OnAnyButtonPress());
    }

    private void OnAnyButtonPress() 
    {
        timeUntilInactive = timeAllowedInactive;
        SetIsInRestartCountdown(false);
    }

    private void SetIsInRestartCountdown(bool value)
    {
        if (isInRestartCountdown != value)
        {
            isInRestartCountdown = value;
            backgroundPanel.gameObject.SetActive(isInRestartCountdown);

            if (isInRestartCountdown)
            {
                timeUntilRestart = restartDuration;
                animateAlphaCoroutine = StartCoroutine(AnimatePanelAlpha(restartDuration));
            }
            else if (animateAlphaCoroutine != null)
            {
                StopCoroutine(animateAlphaCoroutine);
            }
        }
    }

    private IEnumerator AnimatePanelAlpha(float duration)
    {
        float t = 0;

        while (t < duration)
        {
            canvasGroup.alpha = t * 2;
            Color c = backgroundPanel.color;
            c.a = Mathf.Lerp(backgroundPanelAlphaStart, backgroundPanelAlphaEnd, t / duration);
            backgroundPanel.color = c;

            yield return null;
            t += Time.deltaTime;
        }

        canvasGroup.alpha = 1;
        Color col = backgroundPanel.color;
        col.a = backgroundPanelAlphaEnd;
        backgroundPanel.color = col;

        animateAlphaCoroutine = null;
    }

    public void Restart()
    {
        OnAnyButtonPress();

        Debug.Log("[Convention] Restarting...");
        
        myQuitHandler.LoadMainMenu();

        Debug.Log("[Convention] Deleting profiles...");

        SaveSystem.DeleteSaveProfile(0);
        SaveSystem.DeleteSaveProfile(1);
        SaveSystem.DeleteSaveProfile(2);
    }
}
