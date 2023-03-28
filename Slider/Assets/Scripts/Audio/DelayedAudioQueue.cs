using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AT: this is more of a hack than an actual solution
// Cinemachine updates things in late update but the execution order could not be edited
// But AudioManager's execution order was already tied to other stuff to make sure initialization goes correctly
// The only way to have something after cinemachine is to create this separate script
public class DelayedAudioQueue : MonoBehaviour
{
    private List<(FMODUnity.EventReference, Vector3, Vector3)> oneshots = new List<(FMODUnity.EventReference, Vector3, Vector3)>();

    private void LateUpdate()
    {
        foreach(var (reference, position, original) in oneshots)
        {
            FMODUnity.RuntimeManager.PlayOneShot(reference, position + (original - Camera.main.transform.position));
        }
        oneshots.Clear();
    }

    public void EnqueueOneshot(FMODUnity.EventReference reference, Vector3 position, Vector3 originalCameraPosition)
    {
        oneshots.Add((reference, position, originalCameraPosition));
    }
}
