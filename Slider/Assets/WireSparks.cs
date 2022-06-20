using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireSparks : MonoBehaviour
{
    [SerializeField] private float minInterval;
    [SerializeField] private float maxInterval;
    [SerializeField] [Range(0f, 1f)] private float probSmall;
    [SerializeField] private Animator anim;

    // Start is called before the first frame update
    private void OnEnable()
    {
        StartCoroutine(DoSparks());
    }

    private IEnumerator DoSparks()
    {
        while (true)
        {
            if (Random.Range(0f, 1f) <= probSmall)
            {
                anim.SetTrigger("SparksSmall");
            }
            else
            {
                anim.SetTrigger("SparksNormal");
            }

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }
}
