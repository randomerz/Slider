using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitySaveManager : MonoBehaviour
{
    private void GetSavableObjects()
    {
        foreach (var obj in FindObjectsOfType<MonoBehaviour>(true).OfType<ISavable>())
        {
            // save
        }
    }
}
