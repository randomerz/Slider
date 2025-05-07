using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How to use TransformInterpolator properly:
/// 0. Make sure the gameobject executes its mechanics (transform-manipulations)
/// in FixedUpdate().
/// 1. Make sure VSYNC is enabled.
/// 2. Set the execution order for this script BEFORE all the other scripts
/// that execute mechanics.
/// 3. Attach (and enable) this component to every gameobject that you want to interpolate
/// (including the camera).
/// </summary>
public class TransformInterpolator : MonoBehaviour
{
    private Vector3 prevPosition;
    private Vector3 currentPosition;
    private bool isTransformInterpolated = false;

    void OnEnable()
    {
        prevPosition = transform.position;
        isTransformInterpolated = false;
    }

    void FixedUpdate()
    {
        // Reset transform to its supposed current state just once after each Update/Drawing.
        if (isTransformInterpolated)
        {
            transform.position = currentPosition;

            isTransformInterpolated = false;
        }

        // Cache current transform state as previous
        // (becomes "previous" by the next transform-manipulation
        // in FixedUpdate() of another component).
        prevPosition = transform.position;
    }

    void LateUpdate()   // Interpolate in Update() or LateUpdate().
    {
        // Cache the updated transform so that it can be restored in
        // FixedUpdate() after drawing.
        if (!isTransformInterpolated)
        {
            currentPosition = transform.position;

            // This promise matches the execution that follows after that.
            isTransformInterpolated = true;
        }

        // (Time.time - Time.fixedTime) is the "unprocessed" time according to documentation.
        float interpolationAlpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

        // Interpolate transform:
        transform.position = Vector3.Lerp(prevPosition, currentPosition, interpolationAlpha);
    }
}