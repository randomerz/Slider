using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationInjector : MonoBehaviour
{
    void Start()
    {
        LocalizationLoader.LocalizePrefab(gameObject);
    }
}
