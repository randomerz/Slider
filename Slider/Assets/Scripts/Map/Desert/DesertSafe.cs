using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertSafe : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mirageSpriteRenderer;
    [SerializeField] private Sprite meltedMirageSprite;

    [SerializeField] private float timeToBeLaseredForMelt;
    [SerializeField] private DinoLasersManager dinoLasersManager;

    private bool currentlyLasered = false;
    private bool melted = false;

    private bool laseredLastUpdate = false;
    private float laseredStartTime;
    public GameObject laseredSafeUI;

    private void Start()
    {
        if (SaveSystem.Current.GetBool("desertSafeMelted"))
        {
            MeltSafe();
        }
    }

    public void OnLasered()
    {
        //Debug.Log("Safe lasered!");
        currentlyLasered = true;
    }

    public void OnUnLasered()
    {
        // Debug.Log("Safe Unlasered!");
        currentlyLasered = false;
    }

    private void Update()
    {
        if (!melted)
        {
            if (currentlyLasered && !laseredLastUpdate)
            {
                laseredLastUpdate = true;
                laseredStartTime = Time.time;
            }
            else if (currentlyLasered && laseredLastUpdate)
            {
                if (Time.time - laseredStartTime > timeToBeLaseredForMelt)
                {
                    MeltSafe();
                    return;
                }
            }
            else if (!currentlyLasered && laseredLastUpdate)
            {
                laseredLastUpdate = false;
            }
        }
    }

    private void MeltSafe()
    {
        melted = true;

        mirageSpriteRenderer.sprite = meltedMirageSprite;
        if (!SaveSystem.Current.GetBool("desertSafeMelted"))
        {
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position + Vector3.up * 0.5f);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position + Vector3.right * 0.5f);
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position + Vector3.left * 0.5f);
            StartCoroutine(MeltSafeAudio());
        }

        SaveSystem.Current.SetBool("desertSafeMelted", true);

        dinoLasersManager.RemoveAllLasersPermanently();
        laseredSafeUI.SetActive(true);
    }

    private IEnumerator MeltSafeAudio()
    {
        AudioManager.PlayWithVolume("Ice Melt", 1f);
        yield return new WaitForSeconds(1f);
        AudioManager.PlayWithVolume("Ice Melt", 0.75f);
        yield return new WaitForSeconds(1f);
        AudioManager.PlayWithVolume("Ice Melt", 0.5f);
    }
}
