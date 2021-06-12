using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public Item currentHeldItem; // for rope puzzle

    // Movement
    public float moveSpeed = 5;

    private Vector3 inputDir;

    void Start()
    {
        
    }
    
    void Update()
    {
        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate()
    {
        transform.position += moveSpeed * inputDir.normalized * Time.deltaTime;
    }
}
