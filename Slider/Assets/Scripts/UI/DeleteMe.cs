using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteMe : MonoBehaviour
{
    [SerializeField] private UIMenu firstMenu;

    void Start()
    {
        firstMenu.Open();
    }
}
