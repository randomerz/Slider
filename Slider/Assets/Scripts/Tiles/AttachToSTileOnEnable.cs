using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToSTileOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        TileUtil.AttachToTileUnderneath(gameObject);
    }
}
