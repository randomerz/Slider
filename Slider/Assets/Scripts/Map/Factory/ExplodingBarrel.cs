using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExplodingBarrel : MonoBehaviour
{
    private readonly string explosionResName = "SmokePoof Variant";
    
    [SerializeField] private Transform[] explosionLocations;

    public UnityEvent OnExplode;

    private GameObject explosionEffect;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        explosionEffect = Resources.Load<GameObject>(explosionResName);
    }
    public void Explode()
    {
        StartCoroutine(ExplodeRoutine());
    }

    private IEnumerator ExplodeRoutine()
    {
        //Do Ze Animation
        yield return new WaitForSeconds(2.0f);
        foreach (Transform location in explosionLocations)
        {
            Instantiate(explosionEffect, location.position, Quaternion.identity);
        }
        Destroy(gameObject);

        OnExplode?.Invoke();
    }
}