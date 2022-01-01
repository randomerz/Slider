using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGridBackground : MonoBehaviour
{
    public GameObject[] borderColliders; // right top left bottom




    public void SetBorderCollider(int index, bool isActive)
    {
        borderColliders[index].SetActive(isActive);
    }

    public void SetBorderColliders(bool isActive)
    {
        foreach (GameObject g in borderColliders)
            g.SetActive(isActive);
    }
}
