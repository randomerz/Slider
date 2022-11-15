using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMovePlayerSprite : MonoBehaviour
{
    public GameObject playerSprite;

    private void OnEnable() {
        Reset();
    }

    private void OnDisable() {
        Reset();
    }

    public void MoveDown()
    {
        playerSprite.transform.localPosition = new Vector3(0, -1f / 16f);
    }

    public void Reset()
    {
        playerSprite.transform.localPosition = Vector3.zero;
    }
}
