using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static GameObject[] items = new GameObject[10];
    // Start is called before the first frame update
    void Start()
    {
        items[9].SetActive(false);
    }

    public static void ActivateDigItem()
    {
        if (NPCManager.CheckQRCode())
        {
            items[0].SetActive(true);
        }
    }
}
