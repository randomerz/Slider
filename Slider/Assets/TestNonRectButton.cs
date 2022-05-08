using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestNonRectButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        Debug.Log(this.GetComponent<Image>().alphaHitTestMinimumThreshold);
    }

    // Update is called once per frameS
    void Update()
    {
        
    }
}
