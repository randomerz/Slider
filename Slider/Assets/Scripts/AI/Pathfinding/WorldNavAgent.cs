using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//Attach this to the agent you want to use STileNavigation (or WorldNavigation)
public class WorldNavAgent : MonoBehaviour
{
    public float speed;
    public float tolerance;

    private List<Vector2Int> path;

    private Vector2Int currDest;

    private Coroutine followRoutine;

    private Rigidbody2D rb;

    private WorldNavigation nav;

    public bool IsRunning
    {
        get;
        private set;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("WorldNavAgent requires Rigidbody2D");
        }

        nav = GetComponentInParent<WorldNavigation>();
        if (nav == null)
        {
            Debug.LogWarning("WorldNavAgent detected in scene without a WorldNavigation component. This entity will not use navigation.");
        }
    }

    public bool SetDestination(Vector2Int dest, Func<Vector2Int, Vector2Int, Vector2Int, int> costFunc = null, Action<Vector2Int> callback = null)
    {
        //Get the closest point to this transform
        Vector2Int posAsInt = TileUtil.WorldToTileCoords(transform.position);
        nav.GetPathFromToAStar(posAsInt, dest, out path, false, costFunc);
        //Debug.Log($"Path To {dest}:");
        //foreach (Vector2Int pos in path)
        //{
        //    Debug.Log(pos);
        //}

        if (IsRunning)
        {
            StopPath();
        }

        if (path == null || path.Count == 0)
        {
            return false;
        }
        currDest = dest;
        followRoutine = StartCoroutine(FollowCoroutine(callback));
        return true;
    }

    private IEnumerator FollowCoroutine(Action<Vector2Int> callback = null)
    {
        IsRunning = true;
        while(path.Count > 0)
        {
            while (Vector2.Distance(path[0], transform.position) > tolerance)
            {
                rb.velocity = (path[0] - (Vector2)transform.position).normalized * speed;
                yield return null;
            }

            path.RemoveAt(0);
        }

        while (Vector2.Distance(transform.position, currDest) > tolerance)
        {
            rb.velocity = (currDest - (Vector2)transform.position).normalized * speed;
            yield return null;
        }

        IsRunning = false;
        rb.velocity = Vector2.zero;

        if (callback != null)
        {
            callback(currDest);
        }
    }

    public void StopPath()
    {
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
        }

        IsRunning = false;
        rb.velocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        //Draws out the path
        if (path != null)
        {
            for (int i=0; i<path.Count; i++)
            {
                Gizmos.color = Color.green;
                //Gizmos.DrawSphere(new Vector3(path[i].x, path[i].y, 0), 0.2f);

                if (i != path.Count - 1)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(new Vector3(path[i].x, path[i].y, 0), new Vector3(path[i+1].x, path[i+1].y, 0));
                }
            }
        }
    }
}