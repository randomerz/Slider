using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrail : MonoBehaviour 
{
    public float distanceBetweenParticles = 2;
    public float delayBetweenParticles = 0.1f;
    public float delayBetweenTrailRepeat = 8;
    public bool doAudio = true;

    public GameObject particlePrefab;
    public Transform trailStart;
    public Transform trailTarget;

    private Coroutine repeatTrailCoroutine;

    public void SpawnParticleTrail(bool shouldRepeat=false)
    {
        List<Vector3> particlePositions = new List<Vector3>();
        Vector3 dif = trailTarget.position - trailStart.position;
        Vector3 dir = dif.normalized;
        int numParticles = (int)(dif.magnitude / distanceBetweenParticles) + 1;

        for (int t = 0; t < numParticles; t++)
        {
            Vector3 newPos = trailStart.position + t * distanceBetweenParticles * dir;
            particlePositions.Add(newPos);
        }

        if (shouldRepeat)
        {
            repeatTrailCoroutine = StartCoroutine(RepeatTrailSpawns(particlePositions));
        }
        else
        {
            StartCoroutine(StaggerParticleSpawns(particlePositions));
        }
    }

    private IEnumerator RepeatTrailSpawns(List<Vector3> positions)
    {
        while (true)
        {
            StartCoroutine(StaggerParticleSpawns(positions));

            yield return new WaitForSeconds(delayBetweenTrailRepeat);
        }
    }

    public void StopRepeating()
    {
        if (repeatTrailCoroutine != null) StopCoroutine(repeatTrailCoroutine);
        repeatTrailCoroutine = null;
    }

    private IEnumerator StaggerParticleSpawns(List<Vector3> positions)
    {
        foreach (Vector3 p in positions)
        {
            Instantiate(particlePrefab, p, Quaternion.identity, transform);
            if (doAudio)
            {
                AudioManager.PlayWithVolume("Hat Click", 0.5f);
            }

            yield return new WaitForSeconds(delayBetweenParticles);
        }
    }
}