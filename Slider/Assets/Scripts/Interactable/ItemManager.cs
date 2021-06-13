using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject[] items = new GameObject[10];

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < items.Length; x++)
        {

            items[x].SetActive(false);
        }
    }
}
