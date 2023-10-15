using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetControlsButton : MonoBehaviour
{
    public void OnClick()
    {
        Controls.ResetAllBindingsToDefaults();
        InputRebinding.OnRebindCompleted?.Invoke();
    }
}
