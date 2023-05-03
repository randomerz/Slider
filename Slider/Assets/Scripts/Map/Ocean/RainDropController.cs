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

    [Header("Parameters")]
    [SerializeField] private float dropsPerSecond;
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
    }

    private void SpawnRainDrop()
    {
        Vector3 pos = GetRandomPosition();
        if (Vector3.Distance(pos, Player.GetPosition()) < 40)
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
