using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakableTree : MonoBehaviour
{

    public bool isShaken;

    // Start is called before the first frame update
    void Start()
    {
        isShaken = false;
    }

    private void Awake()
    {
        isShaken = false;
    }

    public void shakeTree()
    {
        if (isShaken)
        {
            Debug.Log("already shaken");
        } else
        {
            Debug.Log("you shake tree");
            isShaken = true;
        }
        
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
