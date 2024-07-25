using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogProp : MonoBehaviour
{
    public const float BARK_COOLDOWN = 0.2f;
    private float timeUntilCanBark = 0;

    public GameObject emoteGameObject;

    private void Update()
    {
        timeUntilCanBark -= Time.deltaTime;
    }

    public void Bark()
    {
        if (timeUntilCanBark <= 0)
        {
            timeUntilCanBark = BARK_COOLDOWN;
            float randomPitch = Random.Range(0.8f, 1.1f);
            string sound = Random.Range(0, 2) == 0 ? "Dog Bark 1" : "Dog Bark 2";
            AudioManager.PickSound(sound).WithPitch(randomPitch).WithVolume(0.5f).WithAttachmentToTransform(transform).AndPlay();
            
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
