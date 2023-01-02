using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleLight : MonoBehaviour
{
    private float scale;
    public float delta = 0.1f;
    public float flickerSpeed = 0.5f;
    private float currScale;
    private float randOffset;

    private void Awake() {
        scale = transform.localScale.x;
        randOffset = Random.Range(0, 2 * Mathf.PI);
    }

    private void Update() {
        currScale = scale + delta * Mathf.Sin(Time.time * flickerSpeed + randOffset);
        transform.localScale = new Vector3(currScale, currScale, 1);
    } 
}
