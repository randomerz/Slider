using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PipeLiquid : MonoBehaviour
{
    [FormerlySerializedAs("FILLDURATION")]
    [SerializeField] private float fillDuration = 5f;
    [SerializeField] private LineRenderer lineRenderer;
    private Vector3[] points;
    private float[] distance; 

    public bool isFilling;
    public bool isFull;

    public UnityEvent OnIsFull;
    public UnityEvent OnIsEmpty;
    public UnityEvent OnStartFill;

    private bool didInit;

    public bool updateRTPos;
    public Material gooMaterial;
    public Vector4 rtPos;
    public Vector3 initialPos;
    
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (didInit)
        {
            return;
        }
        didInit = true;

        SavePoints();
        SetPipeEmpty();
    }

    private void LateUpdate()
    {
        if(updateRTPos) 
            UpdateRTPos();
    }
    
    private void UpdateRTPos()
    {
        Vector3 diff = transform.position - initialPos;
        Vector4 newRtPos = rtPos + new Vector4(diff.x, diff.y);
        gooMaterial.SetVector("_RTOffset", newRtPos);
    }

    private void OnDisable()
    {
        if(updateRTPos)
            gooMaterial.SetVector("_RTOffset", rtPos);
    }

    private void SavePoints()
    {
        int numPoints = lineRenderer.positionCount;
        points = new Vector3[numPoints];
        lineRenderer.GetPositions(points);

        distance = new float[numPoints];
        float totalDistance = 0;
        for(int i = 1; i < numPoints; i++)
        {
            Vector3 currPoint = points[i];
            Vector3 prevPoint = points[i-1];
            float d = Vector3.Distance(prevPoint, currPoint);
            totalDistance += d;
            distance[i] = totalDistance;
        }
        
        for(int i = 0; i < distance.Length; i++)
        {
            distance[i] /= totalDistance;
        }
    }

    private IEnumerator AnimateFill(Vector2 startState, Vector2 endState, float duration)
    {
        if (!didInit)
        {
            Init();
        }

        isFull = false;
        isFilling = true;
        OnStartFill?.Invoke();
        Fill(startState);
        float t = 0;
        while (t < duration)
        {
            Vector2 state = Vector2.Lerp(startState, endState, t/duration);
            Fill(state);
            t += Time.deltaTime;
            yield return null;
        }
        Fill(endState);
        isFilling = false;
        if(endState == Vector2.up)
        {
            isFull = true;
            OnIsFull?.Invoke();
        }
        else if (endState[0] == endState[1])
        {
            OnIsEmpty?.Invoke();
        }
    }

    public void Fill(Vector2 state)
    {
        Fill(state[0], state[1]);
    }

    //Fills the segment of the pipe (0-1) from start to end.
    private void Fill(float startPos, float endPos)
    {
        if (!didInit)
        {
            Init();
        }
        
        if (startPos >= endPos)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        List<Vector3> newPositions = new();

        for (int i = 0; i < distance.Length - 1; i++)
        {
            float currDistance = distance[i];
            float nextDistance = distance[i + 1];
            Vector3 currStart = points[i];
            Vector3 currEnd = points[i + 1];

            if (endPos < currDistance || startPos > nextDistance) //not included
                continue;
            else if (startPos < currDistance) //include start
            {
                newPositions.Add(currStart);
                if (endPos < nextDistance)
                {
                    float t = (endPos - currDistance) / (nextDistance - currDistance);
                    Vector3 v = Vector3.Lerp(currStart, currEnd, t);
                    newPositions.Add(v);
                }
            }
            else if (startPos < nextDistance)
            {
                float t = (startPos - currDistance) / (nextDistance - currDistance);
                Vector3 v = Vector3.Lerp(currStart, currEnd, t);
                newPositions.Add(v);

                if(endPos < nextDistance)
                {
                    float t2 = (endPos - currDistance) / (nextDistance - currDistance);
                    Vector3 v2 = Vector3.Lerp(currStart, currEnd, t2);
                    newPositions.Add(v2);
                }
            }
        }
        if(endPos == 1)
            newPositions.Add(points[^1]);
        
        lineRenderer.positionCount = newPositions.Count;
        lineRenderer.SetPositions(newPositions.ToArray());
    }

    public void FillPipe()
    {
        FillPipe(fillDuration);
    }

    public void FillPipe(float duration)
    {
        StartCoroutine(AnimateFill(Vector2.zero, Vector2.up, duration));
    }

    public void FillPipe(Vector2 start, Vector2 end, float duration)
    {
        StartCoroutine(AnimateFill(start, end, duration));
    }

    public void SetPipeFull()
    {
        Fill(Vector2.up);
        isFull = true;
        OnIsFull?.Invoke();
    }

    public void SetPipeEmpty()
    {
        Fill(Vector2.zero);
        OnIsEmpty?.Invoke();
    }

    public void EmptyPipeStartToEnd()
    {
        StartCoroutine(AnimateFill(Vector2.up, Vector2.one, fillDuration));
    }

    public void EmptyPipeEndToStart()
    {
        StartCoroutine(AnimateFill(Vector2.up, Vector2.zero, fillDuration));
    }

    public void IsPipeFilling(Condition c)
    {
        c.SetSpec(isFilling);
    }

    public void IsPipeFull(Condition c)
    {
        c.SetSpec(isFull);
    }
}
