using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CameraDolly : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineTrackedDolly dolly;
    public CinemachineSmoothPath path;
    private int numWaypoints;
    
    public AnimationCurve pathMovementCurve;
    public float duration;

    private BindingHeldBehavior dollySkipBindingBehavior;

    [SerializeField] private Slider skipPrompt;
    [SerializeField] private TextMeshProUGUI skipPromptText;
    [SerializeField] private float holdDurationToSkip = 1.5f;

    private void Awake() 
    {
        dolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        numWaypoints = path.m_Waypoints.Length;
    }

    public void StartTrack()
    {
        virtualCamera.Priority = 15;

        dollySkipBindingBehavior = new BindingHeldBehavior(
                Controls.Bindings.Player.Action,
                holdDurationToSkip,
                onHoldStarted: (ignored) => { InitializeSkipPrompt(); },
                onEachFrameWhileButtonHeld: UpdateSkipPrompt,
                onHoldCompleted: SkipToEndOfTrack,
                onButtonReleasedEarly: (ignored) => { InitializeSkipPrompt(); }
        );
        Controls.RegisterBindingBehavior(dollySkipBindingBehavior);

        StartCoroutine(Rollercoaster());
    }

    private void InitializeSkipPrompt()
    {
        skipPromptText.text = $"Hold {Controls.GetBindingDisplayString(Controls.Bindings.Player.Action, onlyShowKey: true)} to Skip";
        skipPrompt.value = 0;
        skipPrompt.gameObject.SetActive(true);
    }

    private void UpdateSkipPrompt(float durationButtonHeldSoFar)
    {
        skipPrompt.value = durationButtonHeldSoFar / holdDurationToSkip;
    }
    
    private IEnumerator Rollercoaster()
    {
        UIEffects.FadeFromBlack();
        UIManager.canOpenMenus = false;
        Player.SetCanMove(false);

        float t = 0;
        
        // fade out at end
        while (t < duration - 0.25f)
        {
            float x = (t / duration);

            dolly.m_PathPosition = pathMovementCurve.Evaluate(x) * (numWaypoints - 1);

            yield return null;
            t += Time.deltaTime;
        }

        UIEffects.FadeToBlack(
            () => EndTrack()
        );

        while (t < duration)
        {
            float x = (t / duration);

            dolly.m_PathPosition = pathMovementCurve.Evaluate(x) * (numWaypoints - 1);

            yield return null;
            t += Time.deltaTime;
        }
    }

    private void SkipToEndOfTrack(InputAction.CallbackContext ignored)
    {
        UIEffects.FadeToBlack(
            () => EndTrack()
        );
        StopAllCoroutines(); // Stops the dolly movement and prevents FadeToBlack from being called twice
    }

    private void EndTrack()
    {
        UIEffects.FadeFromBlack();
        UIManager.canOpenMenus = true;
        Player.SetCanMove(true);
        virtualCamera.Priority = -15;
        skipPrompt.gameObject.SetActive(false);
        Controls.UnregisterBindingBehavior(dollySkipBindingBehavior);
    }
}
