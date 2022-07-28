using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public static event System.EventHandler<OnTimeChangeArgs> OnTimeChange;
    [SerializeField] private bool isInPast;

    public class OnTimeChangeArgs : System.EventArgs
    {
        public bool fromPast;
    }

    private void Start()
    {
        isInPast = transform.position.x > 67;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            OnTimeChange?.Invoke(this, new OnTimeChangeArgs { fromPast = isInPast });
        }
    }
}
