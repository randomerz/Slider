using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireSparks : MonoBehaviour
{
    [SerializeField] private float minInterval;
    [SerializeField] private float maxInterval;
    //[SerializeField] [Range(0f, 1f)] private float probSmall;
    [SerializeField] private Animator anim;

    public void StartSparks(string type)
    {
        StartCoroutine(DoSparks(type));
    }

    private IEnumerator DoSparks(string type)
    {
        while (true)
        {
            anim.SetTrigger(type);

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }
}
