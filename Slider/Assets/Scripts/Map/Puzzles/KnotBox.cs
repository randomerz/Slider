using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class KnotBox : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] knotnodes;
    public Color bad, good;
    public LineRenderer[] lines;

    public ParticleSystem[] particles;
    public Transform[] correctPositions;

    public bool[] linesArr;

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
            // Changing scenes can cause NRE if you are carrying it
            if (lines[i] == null)
            {
                return;
            }

            lines[i].SetPosition(0, Vector3.zero);
            lines[i].SetPosition(1, Vector3.zero);
        }
    }


    //returns a boolean array of if the line at index i is good/bad
    public bool[] CheckLines()
    {
        bool[] ret = new bool[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            bool intersects = false;
            Vector2 a = lines[i].GetPosition(0); 
            Vector2 b = lines[i].GetPosition(1);
            Vector2 dir = b - a;
            int next = i+1;
            if(i == lines.Length-1){
                next = 0;
            } 
            //RaycastHit2D hit = Physics2D.Raycast(a + 0.43f*dir.normalized,dir,(dir.magnitude-0.877f), 2048);
            RaycastHit2D[] hits = Physics2D.RaycastAll(a, dir, dir.magnitude, 4096);
            foreach(RaycastHit2D hit in hits) {
                if(!(GameObject.ReferenceEquals(knotnodes[i], hit.collider.gameObject) || GameObject.ReferenceEquals(knotnodes[next], hit.collider.gameObject))){
                    lines[i].startColor = bad;
                    lines[i].endColor = bad;
                    intersects = true;
                    ret[i] = true;
                }
            }
            for(int j = i-2; j >= 0; j--)
            {
                if(j == 0 && i == lines.Length-1)
                    break;
                if (IntersectingSegs(lines[i], lines[j]))
                {
                    lines[i].startColor = bad;
                    lines[i].endColor = bad;
                    intersects = true;
                    ret[i] = true;
                }
            }
            for (int j = i + 2; j < lines.Length; j++)
            {
                if(j == lines.Length-1 && i == 0)
                    break;
                if (IntersectingSegs(lines[i], lines[j]))
                {
                    lines[i].startColor = bad;
                    lines[i].endColor = bad;
                    intersects = true;
                    ret[i] = true;
                }
            }
            if (!intersects)
            {
                lines[i].startColor = good;
                lines[i].endColor = good;
            }
        }
        linesArr = ret;
        return ret;
    }

    public int CheckNumLines()
    {   
        return CheckLines().Where(c => c).Count();
    }

    public static bool Approximately(float a, float b, float tolerance = 1e-5f) {
        return Mathf.Abs(a - b) <= tolerance;
    }
    private float CrossProduct2D(Vector2 a, Vector2 b){
        return a.x * b.y - a.y * b.x;
    }
    private bool IntersectingSegs(LineRenderer line1, LineRenderer line2)
    {
        // Haha segs
        Vector2 a = line1.GetPosition(0); // line segments a->b and c->d
        Vector2 b = line1.GetPosition(1);
        Vector2 c = line2.GetPosition(0);
        Vector2 d = line2.GetPosition(1);
        
        //orientations of point to points on other line segment
        float oa = CrossProduct2D(d-a,c-a);
        float ob = CrossProduct2D(d-b,c-b);
        float oc = CrossProduct2D(b-c,a-c);
        float od = CrossProduct2D(b-d,a-d);
        if(Mathf.Sign(oa) + Mathf.Sign(ob) == 0 && Mathf.Sign(oc) + Mathf.Sign(od) == 0) {
            return true;
        }
        return false;
    }

    public void CheckParticles()
    {
        if (CheckNumLines() == 0 && particles != null)
        {
            foreach (ParticleSystem ps in particles)
            {
                ps.Play();
            }
            if (SGrid.Current is VillageGrid)
            {
                AudioManager.Play("Hat Click");
            }
        }
    }

    public void SetToCorrectPositions()
    {
        for (int i = 0; i < knotnodes.Length; i++)
        {
            // knotnodes[i] is the sprite in village, so we want the parent
            knotnodes[i].transform.parent.position = correctPositions[i].transform.position;
        }
    }

    public void CheckPuzzle(Condition c)
    {
        c.SetSpec(CheckNumLines() == 0);
    }

    public void CheckPuzzlePartial(Condition c)
    {
        c.SetSpec(CheckNumLines() <= 3);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < lines.Length; i++)
        {
            if (i < knotnodes.Length - 1)
                Gizmos.DrawLine(knotnodes[i].transform.position, knotnodes[i + 1].transform.position);
            else
                Gizmos.DrawLine(knotnodes[i].transform.position, knotnodes[0].transform.position);
        }
    }
}
