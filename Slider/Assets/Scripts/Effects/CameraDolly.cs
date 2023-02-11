using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraDolly : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    protected CinemachineTrackedDolly dolly;
    public CinemachineSmoothPath path;
    protected int numWaypoints;
    
    public AnimationCurve pathMovementCurve;
    public float duration;

    protected BindingHeldBehavior dollySkipBindingBehavior;

    protected void Awake() 
    {
        dolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        numWaypoints = path.m_Waypoints.Length;
    }

    public void StartTrack()
    {
        virtualCamera.Priority = 15;

        dollySkipBindingBehavior = (BindingHeldBehavior) Controls.RegisterBindingBehavior(
            new BindingHeldBehavior(Controls.Bindings.Player.Action, SkipToEndOfTrack, 1.5f));

        StartCoroutine(Rollercoaster());
    }
    
    protected virtual IEnumerator Rollercoaster()
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

    protected void SkipToEndOfTrack(InputAction.CallbackContext ignored)
    {
        UIEffects.FadeToBlack(
            () => EndTrack()
        );
        StopAllCoroutines(); // Stops the dolly movement and prevents FadeToBlack from being called twice
        Controls.UnregisterBindingBehavior(dollySkipBindingBehavior);
    }

    protected void EndTrack()
    {
        UIEffects.FadeFromBlack();
        UIManager.canOpenMenus = true;
        Player.SetCanMove(true);
        virtualCamera.Priority = -15;
    }
}
