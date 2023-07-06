using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STileSoundTransform : MonoBehaviour
{
    public SMove move;
    public float lerp = 0.25f;

    public STileSoundTransform(SMove move, float lerp)
    {
        this.move = move;
        this.lerp = lerp;
    }

    void Update()
    {
        this.transform.position = Vector3.Lerp(move.GetMoveTilesCenter(), Player.GetPosition(), lerp);
    }
}
