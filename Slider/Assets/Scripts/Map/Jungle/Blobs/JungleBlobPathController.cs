using System.Collections.Generic;
using UnityEngine;

public class JungleBlobPathController : MonoBehaviour
{
    private class MarchData 
    {
        public bool isOn;
        public List<Blob> blobs = new();
        public float timeUntilNextSpawn;

        public Shape shape;
        public JungleBox target;
    }

    private Dictionary<Direction, MarchData> marchData = new();
    private bool didInit;

    public GameObject blobPrefab;

    private const float TIME_BETWEEN_SPAWNS = 4;

    private void Awake()
    {
        InitMarchData();
    }

    private void InitMarchData()
    {
        if (didInit)
            return;
        didInit = true;

        foreach (Direction d in DirectionUtil.Directions)
        {
            marchData[d] = new MarchData();
        }
    }

    private void Update()
    {
        foreach (Direction d in DirectionUtil.Directions)
        {
            MarchData md = marchData[d];
            if (md.isOn)
            {
                md.timeUntilNextSpawn -= Time.deltaTime;
                if (md.timeUntilNextSpawn <= 0)
                {
                    SpawnBlob(d, 0);
                }
            }
        }
    }

    private MarchData GetMarchData(Direction direction)
    {
        if (!marchData.ContainsKey(direction))
            InitMarchData();

        return marchData[direction];
    }

    public void EnableMarching(Direction direction, Shape shape, JungleBox target)
    {
        MarchData md = GetMarchData(direction);
        if (md.isOn)
            return;
        md.isOn = true;

        if (transform.position.x != target.transform.position.x &&
            transform.position.y != target.transform.position.y)
        {
            Debug.LogError($"Marching between {transform.parent.name} and {target} is not along a straight line!");
        }

        md.shape = shape;
        md.target = target;

        SpawnBlob(direction, 0);
        PrewarmMarching(direction);
    }

    private void PrewarmMarching(Direction direction)
    {
        MarchData md = GetMarchData(direction);

        float distanceBetweenSpawns = Blob.MARCH_SPEED * TIME_BETWEEN_SPAWNS;
        float currentDistance = distanceBetweenSpawns;
        float targetDistanceTraveled = Vector3.Distance(md.target.transform.position, transform.position);

        for (int i = 0; i < 100 && currentDistance < targetDistanceTraveled; i++)
        {
            Blob blob = SpawnBlob(direction, currentDistance);
            blob.SetAlpha(0);

            currentDistance += distanceBetweenSpawns;
        }
    }

    public void UpdateMarchingShape(Direction direction, Shape shape)
    {
        MarchData md = GetMarchData(direction);
        if (!md.isOn)
            return;
            
        if (shape == null)
        {
            DisableMarching(direction);
            return;
        }

        md.shape = shape;

        for (int i = 0; i < md.blobs.Count; i++)
        {
            Blob b = md.blobs[i];
            if (b == null)
            {
                md.blobs.RemoveAt(i);
                i--;
                continue;
            }

            b.SetShape(shape);
        }
    }

    public void DisableMarching(Direction direction)
    {
        MarchData md = GetMarchData(direction);
        if (!md.isOn)
            return;
        md.isOn = false;

        for (int i = 0; i < md.blobs.Count; i++)
        {
            Blob b = md.blobs[i];
            if (b == null)
            {
                // md.blobs.RemoveAt(i);
                // i--;
                continue;
            }

            b.SetTargetAlpha(0, () => b.RemoveBlob());
        }

        md.blobs.Clear();
    }

    private Blob SpawnBlob(Direction direction, float currentDistanceTraveled)
    {
        MarchData md = GetMarchData(direction);

        md.timeUntilNextSpawn = TIME_BETWEEN_SPAWNS;

        Blob blob = GetBlob();

        blob.transform.parent = transform;
        blob.transform.position = transform.position;
        blob.transform.position += DirectionUtil.D2V3(direction) * currentDistanceTraveled;

        float targetDistanceTraveled = Vector3.Distance(md.target.transform.position, transform.position);

        blob.InitializeParameters(direction, targetDistanceTraveled, currentDistanceTraveled, md.shape, this);
        blob.SetAlpha(1);
        blob.SetTargetAlpha(1);
        
        md.blobs.Add(blob);

        return blob;
    }

    private Blob GetBlob()
    {
        GameObject go = Instantiate(blobPrefab);

        return go.GetComponent<Blob>();
    }

    // Called by blob
    public void RemoveBlob(Blob blob)
    {
        marchData[blob.Direction].blobs.Remove(blob);

        Destroy(blob.gameObject);
    }
}