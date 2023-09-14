using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaserFlashManager : MonoBehaviour
{
    // this is like a fake pool cause we refresh ~every frame
    private List<MagiLaserFlash> flashPool = new List<MagiLaserFlash>();
    private int currentIndex;

    public GameObject flashPrefab;

    public void ResetPool()
    {
        currentIndex = 0;
        foreach (MagiLaserFlash f in flashPool)
        {
            f.spriteRenderer.enabled = false;
        }
    }

    public void PutFlash(Transform transform, Vector3 position, float angle)
    {
        MagiLaserFlash flash = GetFlash();
        flash.transform.SetParent(transform);
        flash.transform.position = position;
        flash.SetAngle(angle);
        flash.spriteRenderer.enabled = true;
    }

    private MagiLaserFlash GetFlash()
    {
        if (currentIndex >= flashPool.Count)
        {
            // Create a new one
            GameObject go = Instantiate(flashPrefab);
            flashPool.Add(go.GetComponent<MagiLaserFlash>());
        }

        return flashPool[currentIndex++];
    }
}
