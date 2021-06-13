using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static GameObject[] items = new GameObject[10];

    public GameObject digObj;

    void Start()
    {
        digObj.SetActive(false);
    }

    public static void ActivateDigItem()
    {
        if (NPCManager.CheckQRCode())
        {
            items[0].SetActive(true);
        }
    }
}
