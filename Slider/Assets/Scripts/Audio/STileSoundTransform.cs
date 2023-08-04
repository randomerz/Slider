using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STileSoundTransform : MonoBehaviour
{
    public SMove move;
    public List<Transform> transforms;
    public float lerp = 0.25f;

    void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (move != null)
        {
            transform.position = Vector3.Lerp(move.GetMoveTilesCenter(), Player.GetPosition(), lerp);
        }
        else if (transform != null)
        {
            Vector3 center = Vector3.zero;
            foreach (Transform t in transforms)
            {
                if (t == null)
                {
                    Debug.LogWarning("Something is null!");
                    continue;
                }
                center += t.position;
            }

            transform.position = center / transforms.Count;
        }
    }
}
