using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanArtifact : UIArtifact
{
    public Queue<int> positionQueue;
    public Queue<bool> CCWQueue;
    public bool rotating = false;

    public new void Awake()
    {
        base.Awake();
        positionQueue = new Queue<int>();
        CCWQueue = new Queue<bool>();
    }

    public new void OnDisable()
    {
        base.OnDisable();
        positionQueue = new Queue<int>();
        CCWQueue = new Queue<bool>();
    }
    
    public override void SelectButton(ArtifactTileButton button) 
    {
        // do nothing
        Debug.Log("ocean does nothing!");
    }
    
    // temporary
    public void RotateTiles(int x, int y, bool rotateCCW)
    {
        List<Vector2Int> SMoveRotateArr = new List<Vector2Int> { 
                new Vector2Int(x, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x + 1, y + 1),
                new Vector2Int(x + 1, y),
            };

        List<ArtifactTileButton> tb = new List<ArtifactTileButton>{
            GetButton(x, y),
            GetButton(x, y + 1),
            GetButton(x + 1, y + 1),
            GetButton(x + 1, y)
        };

        if (rotateCCW) 
        {
            SMoveRotateArr.Reverse();
            tb.Reverse();
        }

        for (int i=3; i>=0; i--)
        {
            int curX = SMoveRotateArr[i].x;
            int curY = SMoveRotateArr[i].y;

            STile[,] grid = SGrid.current.GetGrid();

            if (grid[curX, curY].hasAnchor)
            {
                SMoveRotateArr.RemoveAt(i);
                tb.RemoveAt(i);
            }
        }

        SMove rotate = new SMoveRotate(SMoveRotateArr);
        // todo: if can rotate
        SGrid.current.Move(rotate);
        
        for (int i=0; i<tb.Count; i++)
        {
            tb[i].SetPosition(SMoveRotateArr[(i+1) % tb.Count].x, SMoveRotateArr[(i + 1) % tb.Count].y);
        }
    }

    public void AddQueue(int x, int y, bool CCW)
    {
        if (CCWQueue.Count == 0)
        {
            positionQueue.Enqueue(x);
            positionQueue.Enqueue(y);
            CCWQueue.Enqueue(CCW);
            CheckQueue();
        }
    }

    public new void CheckQueue()
    {
        if (!rotating && CCWQueue.Count != 0)
        {
            RotateTiles(positionQueue.Dequeue(), positionQueue.Dequeue(), CCWQueue.Dequeue());
            StartCoroutine(RotationWait());
        }
        else
        {
            return;
        }
    }

    private IEnumerator RotationWait()
    {
        rotating = true;
        yield return new WaitForSeconds(1f);
        rotating = false;
        CheckQueue();
    }
}