using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCShake : MonoBehaviour
{
    public Transform baseTransform;

    // Start is called before the first frame update

    public void StartShake(float duration)
    {
        StartCoroutine(Shake(duration, .5f));
    }

    public IEnumerator Shake(float duration, float amount)
    {
        float curTime = 0;
        Vector3 origPos = transform.position;

        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;

            float curIntensity = Mathf.Lerp(amount, 0, curTime / duration);
            transform.position = baseTransform.position + Random.insideUnitSphere *.4f;

            curTime += .025f  ;

            yield return new WaitForSeconds(.025f);
        }

        transform.position = baseTransform.position;
    }
}
