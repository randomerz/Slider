using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinWaveAnimation : MonoBehaviour
{
    private Vector3 origOffset;

    [Header("A sin (Bx + C) + D")]
    public float A = 1;
    public float B = 1;
    public float C = 0;
    public float D = 0;

    public float horizontalVelocity = 0;

    void Start()
    {
        origOffset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time;
        origOffset += Vector3.right * horizontalVelocity * Time.deltaTime;
        transform.localPosition = origOffset + Vector3.up * Sin(t);
    }

    private float Sin(float x)
    {
        return A * Mathf.Sin(B * x + C) + D;
    }
}
