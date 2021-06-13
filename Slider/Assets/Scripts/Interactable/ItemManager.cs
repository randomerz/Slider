using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static GameObject[] items = new GameObject[10];

    public GameObject digObj;

    private static ItemManager _instance;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        digObj.SetActive(false);
    }

    public static void ActivateDigItem()
    {
        if (NPCManager.CheckQRCode())
        {
            _instance.digObj.SetActive(true);
        }
    }
}
