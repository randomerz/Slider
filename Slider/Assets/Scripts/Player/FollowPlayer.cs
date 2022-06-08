using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    //private Player player;

    void Start()
    {
        //player = GameObject.FindObjectOfType<Player>();

        //if (player == null)
        //{
        //    Debug.LogError("Couldn't find player!");
        //}
    }

    void Update()
    {
        transform.position = Player.GetPosition();
        //transform.position = player.transform.position;
    }
}
