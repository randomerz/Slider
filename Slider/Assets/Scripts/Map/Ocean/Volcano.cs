using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volcano : MonoBehaviour
{
    public ParticleSystem smokeConstant;
    public ParticleSystem[] smokeIncreasings;
    public ParticleSystem[] smokeExplosions;

    public Collectible rockCollectible;
    [SerializeField] private Transform rockStart;
    [SerializeField] private Transform rockTarget;
    [SerializeField] private float pickUpDuration;
    [SerializeField] private AnimationCurve xPickUpMotion;
    [SerializeField] private AnimationCurve yPickUpMotion;

    private Coroutine explosionCoroutine;

    private void Awake() 
    {
        if (rockCollectible.GetComponent<Collider2D>() == null)
            Debug.LogError("Rock Collectible couldn't find collider.");
    }

    private void Start()
    {
        CheckRockFromSave();
    }

    public void Erupt()
    {
        if (SaveSystem.Current.GetBool("oceanVolcanoErupted"))
        {
            CheckRockFromSave();
            return;
        }

        SaveSystem.Current.SetBool("oceanVolcanoErupted", true);
        explosionCoroutine = StartCoroutine(StartEruption());
    }

    private IEnumerator StartEruption()
    {
        yield return new WaitForSeconds(1);

        AudioManager.Play("Rumble Increase 8s");
        CameraShake.ShakeIncrease(8, 0.75f);

        for (int i = 0; i < 4; i++)
        {
            smokeIncreasings[i].Play();

            yield return new WaitForSeconds(2);
        }

        // pause
        smokeConstant.Stop();
        for (int i = 0; i < 4; i++)
            smokeIncreasings[i].Stop();

        yield return new WaitForSeconds(2);

        // boom
        smokeExplosions[0].Play();
        AudioManager.Play("Slide Explosion");
        AudioManager.PlayWithVolume("Rumble Decrease 5s", 0.5f);
        CameraShake.Shake(1, 2);
        StartCoroutine(ItemDrop());

        yield return new WaitForSeconds(0.1f);
        
        smokeExplosions[1].Play();
        explosionCoroutine = null;
    }

    private IEnumerator ItemDrop()
    {
        rockCollectible.gameObject.SetActive(true);
        rockCollectible.GetComponent<Collider2D>().enabled = false;
        Vector3 start = rockStart.transform.position;

        float t = 0;
        while (t < pickUpDuration)
        {
            float x = xPickUpMotion.Evaluate(t / pickUpDuration);
            float y = yPickUpMotion.Evaluate(t / pickUpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, rockTarget.transform.position.x, x),
                                      Mathf.Lerp(start.y, rockTarget.transform.position.y, y));
            
            rockCollectible.transform.position = pos;

            yield return null;
            t += Time.deltaTime;
        }

        rockCollectible.transform.position = rockTarget.transform.position;
        rockCollectible.GetComponent<Collider2D>().enabled = true;

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, rockTarget.transform.position, rockTarget);
    }

    private void CheckRockFromSave()
    {
        if (!SaveSystem.Current.GetBool("oceanVolcanoErupted"))
        {
            return;
        }

        if (explosionCoroutine != null)
        {
            return;
        }

        if (!PlayerInventory.Contains("Rock", Area.Ocean))
        {
            rockCollectible.transform.position = rockTarget.transform.position;
            rockCollectible.GetComponent<Collider2D>().enabled = true;
        }
    }
}
