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
        StartCoroutine(Shake(duration));
    }

    public IEnumerator Shake(float duration)
    {
        float curTime = 0;

        while (curTime <= duration)
        {
            if (Time.timeScale == 0)
                break;
            transform.position = baseTransform.position + Random.insideUnitSphere *.4f;

            curTime += .025f;

            yield return new WaitForSeconds(.025f);
        }

        transform.position = baseTransform.position;
    }

    public void PlaySound(string name)
    {
        AudioManager.Play(name, transform);
    }
}
