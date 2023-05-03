using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class RainDropController : MonoBehaviour
{
    public GameObject dropPrefab;
    private ObjectPool<RainDrop> dropPool;
    public bool isRaining;

    private float dropTimer;
    private float secondsPerDrop => 1f / dropsPerSecond;
    private float rainMultiplier;

    [Header("Parameters")]
    [SerializeField] private float dropsPerSecond;
    [SerializeField] private float timeToFullRain;
    [SerializeField] private Transform minPosition;
    [SerializeField] private Transform maxPosition;

    private void Awake() 
    {
        dropPool = new ObjectPool<RainDrop>(CreateRainDrop, OnTakeDropFromPool, OnReturnDropToPool);
    }

    private void Update() 
    {
        if (isRaining)
        {
            dropTimer -= Time.deltaTime;

            while (dropTimer < 0)
            {   
                SpawnRainDrop();

                dropTimer += secondsPerDrop;
            }
        }

        if (isRaining && rainMultiplier != 1)
        {
            rainMultiplier = Mathf.Min(1, rainMultiplier + (Time.deltaTime / timeToFullRain));
        }
        else if (!isRaining && rainMultiplier != 0)
        {
            rainMultiplier = Mathf.Max(0, rainMultiplier - (Time.deltaTime / timeToFullRain));
        }
    }

    private void SpawnRainDrop()
    {
        if (Random.Range(0f, 1f) > rainMultiplier)
            return;

        Vector3 pos = GetRandomPosition();
        if (Vector3.Distance(pos, Player.GetPosition()) < 35)
        {
            RainDrop drop = dropPool.Get();
            drop.transform.position = pos;
        }
        else
        {
            // spawn rain drop far away..?
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Mathf.Round(Random.Range(minPosition.position.x, maxPosition.position.x) * 16) / 16, 
            Mathf.Round(Random.Range(minPosition.position.y, maxPosition.position.y) * 16) / 16
        );
    }

    public void SetRainActive(bool value)
    {
        isRaining = value;
        // rainMultiplier should take care of smoothing it
    }

    // Pooling

    private RainDrop CreateRainDrop()
    {
        RainDrop drop = Instantiate(dropPrefab).GetComponent<RainDrop>();
        drop.pool = dropPool;

        return drop;
    }

    private void OnTakeDropFromPool(RainDrop drop)
    {
        drop.transform.SetParent(transform);
        drop.gameObject.SetActive(true);

        drop.ResetDrop();
    }

    private void OnReturnDropToPool(RainDrop drop)
    {
        drop.gameObject.SetActive(false);
        drop.transform.SetParent(transform);
    }


    
}
