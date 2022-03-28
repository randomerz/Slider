using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//Attach this to the agent you want to use STileNavigation
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

    public bool SetDestination(Vector2 dest)
    {
        //Need to change to work with multiple stiles later
        STileNavigation stileNav = GetComponentInParent<STileNavigation>();
        path = stileNav.GetPathFromToHard(new Vector2Int((int)transform.position.x, (int)transform.position.y),
                                            new Vector2Int((int)dest.x, (int)dest.y));

        if (IsRunning)
        {
            StopPath();
        }

        if (path == null || path.Count == 0)
        {
            IsRunning = false;
            return false;
        }

        followRoutine = StartCoroutine(FollowCoroutine());
        return true;
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

    public void StopPath()
    {
        if (followRoutine != null && IsRunning)
        {
            StopCoroutine(followRoutine);
            IsRunning = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Draws out the path
        if (path != null)
        {
            for (int i=0; i<path.Count; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(new Vector3(path[i].x, path[i].y, 0), 0.2f);

                if (i != path.Count - 1)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(new Vector3(path[i].x, path[i].y, 0), new Vector3(path[i+1].x, path[i+1].y, 0));
                }
            }
        }
    }
}