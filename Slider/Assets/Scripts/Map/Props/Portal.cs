using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour
{
    public static event System.EventHandler<OnTimeChangeArgs> OnTimeChange;
    [SerializeField] private bool isInPast;

    [SerializeField] private bool useSpecialEventInstead;
    public UnityEvent SpecialPortalEvent;

    public class OnTimeChangeArgs : System.EventArgs
    {
        public bool fromPast;
    }

    private void Start()
    {
        isInPast = MagiTechGrid.IsInPast(transform);
    }

    public void OnPlayerEnter()
    {
        AudioManager.Play("Portal");
        if (useSpecialEventInstead)
        {
            SpecialPortalEvent?.Invoke();
        }
        else
        {
            OnTimeChange?.Invoke(this, new OnTimeChangeArgs { fromPast = isInPast });
        }
    }
}
