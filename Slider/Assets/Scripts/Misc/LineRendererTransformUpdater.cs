using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererTransformUpdater : MonoBehaviour
{
    [System.Serializable]
    public struct TransformIndexPair
    {
        public int index;
        public Transform transform;
    }

    [SerializeField] private List<TransformIndexPair> transformIndexPairs = new List<TransformIndexPair>();
    [SerializeField] private LineRenderer lineRenderer;

    private bool useWorldSpace;

    private void Start() 
    {
        useWorldSpace = lineRenderer.useWorldSpace;
    }

    void LateUpdate()
    {
        foreach (TransformIndexPair pair in transformIndexPairs)
        {
            if (useWorldSpace)
            {
                lineRenderer.SetPosition(pair.index, pair.transform.position);
            }
            else
            {
                lineRenderer.SetPosition(pair.index, pair.transform.position - transform.position);
            }
        }
    }
}
