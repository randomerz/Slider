using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class STileNavAgent : MonoBehaviour
{
    [SerializeField]
    public float speed;
    [SerializeField]
    public float tolerance;

    private List<Vector2Int> path;

    private Vector2 currDest;

    private Coroutine followRoutine;

    public bool IsRunning
    {
        get;
        private set;
    }

    public void SetDestination(Vector2 dest)
    {
        STileNavigation stileNav = GetComponentInParent<STileNavigation>();

        stileNav.GetPathFromToHard(new Vector2Int((int) transform.position.x, (int) transform.position.y), 
                                   new Vector2Int((int) dest.x, (int) dest.y));

        if (followRoutine == null)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogError("No path found. Did you forget to call SetDestination?");
            }
            followRoutine = StartCoroutine(FollowCoroutine());
        }
    }

    public void UpdatePath()
    {
        SetDestination(currDest);
    }

    private IEnumerator FollowCoroutine()
    {
        IsRunning = true;
        while(path.Count > 0)
        {
            transform.up = (path[0] - (Vector2)transform.position).normalized;
            GetComponent<Rigidbody2D>().velocity = transform.up * speed;
            while (Vector2.Distance(path[0], transform.position) > tolerance)
            {
                yield return null;
            }

            path.RemoveAt(0);
        }

        transform.up = (currDest - (Vector2)transform.position).normalized;
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;
        while (Vector2.Distance(transform.position, currDest) > tolerance)
        {
            yield return null;
        }

        IsRunning = false;
    }

    public void ResumePath()
    {
        UpdatePath();
        FollowPath();
    }

    public void StopPath()
    {
        StopCoroutine(followRoutine);
    }
}

