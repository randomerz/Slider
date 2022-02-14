using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to set up borders for tile movement
public class SGridBackground : MonoBehaviour
{
    // these are the world borders that don't move and prevent you from jumping onto a tile
    public GameObject[] borderColliders; // right top left bottom
    private int[] BorderCounts = new int[4];

    public void SetBorderCollider(int index, bool isActive)
    {
        if (isActive)
            BorderCounts[index] += 1;
        else
            BorderCounts[index] -= 1;
        borderColliders[index].SetActive(BorderCounts[index] > 0);
    }

    public void SetBorderColliders(bool isActive)
    {
        for (int i = 0; i < BorderCounts.Length; i++) {
            SetBorderCollider(i, isActive);
        }
    }
}
