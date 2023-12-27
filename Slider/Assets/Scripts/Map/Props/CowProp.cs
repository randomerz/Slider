using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowProp : MonoBehaviour
{
    public const float MOO_COOLDOWN = 0.1f;
    private float timeUntilCanMoo = 0;

    public GameObject emoteGameObject;

    private void Update()
    {
        timeUntilCanMoo -= Time.deltaTime;
    }

    public void Moo()
    {
        if (timeUntilCanMoo <= 0)
        {
            timeUntilCanMoo = MOO_COOLDOWN;
            AudioManager.Play("Cow Moo", transform);
            
            emoteGameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(TurnOffEmoteAfterDelay(1));
        }
    }

    private IEnumerator TurnOffEmoteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        emoteGameObject.SetActive(false);
    }
}
