using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnchorDropDetect : MonoBehaviour
{
    public UnityEvent onAnchorDrop;


    //C: Using existing solution from Factory, not renaming because bad things happen.
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("ButtonTrigger") && other.GetComponentInParent<Anchor>())
            onAnchorDrop.Invoke();
    }
}
