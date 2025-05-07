using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Requirements:
/// 1. Make sure the gameobject does movement/physics in FixedUpdate().
/// 2. Make sure Vsync is enabled.
/// 3. Make sure execution order for this script is before all the other scripts.
/// </summary>
public class PlayerTransformInterpolator : MonoBehaviour
{
    private Vector3 prevPosition;
    private Vector3 targetPosition;
    private Transform prevParent;
    private Transform targetParent;
    private Vector3 prevParentPosition;
    private Vector3 targetParentPosition;
    private Vector3 fixedParentDelta;
    private bool isCurrentlyInterpolated;

    [SerializeField] private Transform positionTruthTransform;

    void OnEnable()
    {
        prevPosition = positionTruthTransform.position;
        isCurrentlyInterpolated = false;
    }

    void FixedUpdate()
    {
        if (isCurrentlyInterpolated)
        {
            // Finish interpolating as the next physics calc moves the player
            transform.position = targetPosition;
            isCurrentlyInterpolated = false;
        }

        if (!ShouldInterpolate()) 
        {
            return;
        }

        prevPosition = positionTruthTransform.position;
        prevPosition.z = transform.position.z;
        prevParent = positionTruthTransform.parent;
        prevParentPosition = prevParent != null ? prevParent.position : Vector3.zero;
    }

    void LateUpdate()
    {
        if (!ShouldInterpolate()) {
            return;
        }

        if (!isCurrentlyInterpolated)
        {
            targetPosition = positionTruthTransform.position;
            targetPosition.z = transform.position.z;
            targetParent = positionTruthTransform.parent;
            targetParentPosition = targetParent != null ? targetParent.position : Vector3.zero;
            isCurrentlyInterpolated = true;

            if (prevParent == targetParent) 
            {
                // Sliders are animated on Update(). We track the prevParentPosition and 
                // targetParentPosition which are snapshot on the fixedUpdateFrame and use those to
                // offset movements to the player caused by the environment.
                fixedParentDelta = targetParentPosition - prevParentPosition;
                prevPosition += fixedParentDelta;
            }
        }

        Vector3 newPrevPos = prevPosition;
        Vector3 newTargetPos = targetPosition;

        // If we're on a Slider for example that is moving, we don't want to interpolate the
        // raw positions but the local positions.
        if (prevParent == targetParent) 
        {
            // Sliders are animated on Update() so we need to keep checking the parent's position.
            // However, the difference between prevPosition and targetPosition also counts the
            // parent's movement, so we track the prevParentPosition, targetParentPosition which
            // are snapshot on the fixedUpdateFrame as well as the current position of the parent.
            Vector3 currentParentPosition = targetParent != null ? targetParent.position : Vector3.zero;
            Vector3 currentParentDelta = currentParentPosition - targetParentPosition;
            newPrevPos += currentParentDelta;
            newTargetPos += currentParentDelta;
        }

        // (Time.time - Time.fixedTime) is the "unprocessed" time according to documentation.
        float interpPercent = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

        transform.position = Vector3.Lerp(newPrevPos, newTargetPos, interpPercent);
    }

    private bool ShouldInterpolate() 
    {
        return (bool)SettingsManager.Setting<bool>(Settings.HighFpsSmoothing).GetCurrentValue();
    }
}