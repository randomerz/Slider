using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static GameObject[] items = new GameObject[9];
    public GameObject[] itemsC = new GameObject[9];
    public GameObject digObj;
    public static int currItemActive = 2;

    private static ItemManager _instance;

    private void Awake()
    {
        _instance = this;
        for (int x = 0; x < items.Length; x++)
        {
            items[x] = itemsC[x];
        }
    }

    void Start()
    {
        for (int x = 0; x < items.Length; x++)
        {
            if (x < 3)
            {
                continue;
            }
            items[x].SetActive(false);
        }
        digObj.SetActive(false);
    }

    public static void ActivateDigItem()
    {
        if (NPCManager.CheckQRCode())
        {
            _instance.digObj.SetActive(true);
        }
    }

    public static void ActivateNextItem()
    {
        currItemActive++;
        items[currItemActive].SetActive(true);
    }
}
