using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private bool followPlayerSpriteInstead;

    void LateUpdate()
    {
        Vector3 position = Player.GetPosition();
        if (followPlayerSpriteInstead && !GameUI.instance.isMenuScene)
        {
            position = Player.GetSpriteRenderer().transform.position;
        }
        transform.position = position;
    }
}
