using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotBox : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] knotnodes;
    public Color bad, good;
    public LineRenderer[] lines;

    // Update is called once per frame
    void Update()
    {
        MakeLines();
        CheckLines();
    }

    // animation is jittery when moving tiles
    // https://answers.unity.com/questions/1387219/linerenderer-lags-behind-object-positions.html
    private void LateUpdate() 
    {
        MakeLines();
        CheckLines();
    }

    private void OnDisable()
    {
        RemoveLines();
    }


    private void MakeLines()
    {
        for (int i = 0; i < knotnodes.Length; i++)
        {
            lines[i].SetPosition(0, knotnodes[i].transform.position);
            if (i < knotnodes.Length - 1)
                lines[i].SetPosition(1, knotnodes[i+1].transform.position);
            else
                lines[i].SetPosition(1, knotnodes[0].transform.position);
        }
    }

    public void RemoveLines()
    {
        for (int i = 0; i < knotnodes.Length; i++)
        {
            lines[i].SetPosition(0, Vector3.zero);
            lines[i].SetPosition(1, Vector3.zero);
        }
    }


    public bool CheckLines()
    {
        bool ret = true;
        for (int i = 0; i < lines.Length; i++)
        {
            bool intersects = false;
            for(int j = 0; j < i; j++)
            {
                if (IntersectingSegs(lines[i], lines[j]))
                {
                    lines[i].startColor = bad;
                    lines[i].endColor = bad;
                    intersects = true;
                    ret = false;
                }
            }
            for (int j = i + 1; j < lines.Length; j++)
            {
                if (IntersectingSegs(lines[i], lines[j]))
                {
                    lines[i].startColor = bad;
                    lines[i].endColor = bad;
                    intersects = true;
                    ret = false;
                }
            }
            if (!intersects)
            {
                lines[i].startColor = good;
                lines[i].endColor = good;
            }
        }
        return ret;
    }

    private bool IntersectingSegs(LineRenderer line1, LineRenderer line2)
    {
        // Haha segs
        Vector2 sp1 = line1.GetPosition(0); //p0
        Vector2 ep1 = line1.GetPosition(1); //p1
        Vector2 sp2 = line2.GetPosition(0); //p2
        Vector2 ep2 = line2.GetPosition(1); //p3

        Vector2 e = ep1 - sp1;
        Vector2 f = ep2 - sp2;
        Vector2 p = new Vector2(-e.y, e.x);
        float h = Vector2.Dot(sp1 - sp2, p) / Vector2.Dot(f, p);

        if (h > 0 && h < 1)
            return true;
        return false;
    }

    public void CheckPuzzle(Conditionals.Condition c)
    {
        c.SetSpec(CheckLines());
    }
}
