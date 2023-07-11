using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnchorDropDetect : MonoBehaviour
{
    public UnityEvent onAnchorDrop;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("ButtonTrigger") && other.GetComponentInParent<Anchor>())
            onAnchorDrop.Invoke();
    }
}
