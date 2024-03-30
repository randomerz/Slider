using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CameraDolly : MonoBehaviour
{
    public System.EventHandler<System.EventArgs> OnRollercoasterEnd;

    public CinemachineVirtualCamera virtualCamera;
    protected CinemachineTrackedDolly dolly;
    public CinemachineSmoothPath path;
    protected int numWaypoints;
    
    public AnimationCurve pathMovementCurve;
    public float duration;

    protected BindingHeldBehavior dollySkipBindingBehavior;
    
    [SerializeField] private Slider skipPromptSlider;
    [SerializeField] private TextMeshProUGUI skipPromptText;
    [SerializeField] private float holdDurationToSkip = 1f;
    [SerializeField] private AnimationCurve holdAnimationCurve;

    protected void Awake()
    {
        dolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        numWaypoints = path.m_Waypoints.Length;
    }

    public void StartTrack(bool DontReturnToPlayerOnEnd = false)
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

        StartCoroutine(Rollercoaster(DontReturnToPlayerOnEnd));
    }

    private void InitializeSkipPrompt()
    {
        // skipPromptText.text = $"Hold {Controls.GetBindingDisplayString(Controls.Bindings.Player.Action, onlyShowKey: true)} to Skip";
        skipPromptText.text = $"Skip";
        skipPromptSlider.value = 0;
        skipPromptSlider.gameObject.SetActive(true);
    }

    private void UpdateSkipPrompt(float durationButtonHeldSoFar)
    {
        skipPromptSlider.value = holdAnimationCurve.Evaluate(durationButtonHeldSoFar / holdDurationToSkip);
    }

    protected virtual IEnumerator Rollercoaster(bool DontReturnToPlayerOnEnd = false)
    {
        UIEffects.FadeFromBlack();
        PauseManager.AddPauseRestriction(owner: gameObject);

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
        
        if(DontReturnToPlayerOnEnd)
        {
            EndTrack(DontReturnToPlayerOnEnd);
        }
        else
            {
            UIEffects.FadeToBlack(
                () => EndTrack(DontReturnToPlayerOnEnd)
            );

            while (t < duration)
            {
                float x = (t / duration);

                dolly.m_PathPosition = pathMovementCurve.Evaluate(x) * (numWaypoints - 1);

                yield return null;
                t += Time.deltaTime;
            }
        }
    }

    protected void SkipToEndOfTrack(InputAction.CallbackContext ignored)
    {
        UIEffects.FadeToBlack(
            () => EndTrack()
        );
        StopAllCoroutines(); // Stops the dolly movement and prevents FadeToBlack from being called twice
    }

    protected void EndTrack(bool DontReturnToPlayerOnEnd = false)
    {
        if (!DontReturnToPlayerOnEnd)
        {
            UIEffects.FadeFromBlack();
            PauseManager.RemovePauseRestriction(owner: gameObject);
            Player.SetCanMove(true);
            virtualCamera.Priority = -15;
        }
        OnRollercoasterEnd?.Invoke(this, null);
        skipPromptSlider.gameObject.SetActive(false);
        Controls.UnregisterBindingBehavior(dollySkipBindingBehavior);
    }
}
