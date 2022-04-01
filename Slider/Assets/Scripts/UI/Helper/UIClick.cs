using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClick : MonoBehaviour
{
    public void Click()
    {
        AudioManager.Play("UI Click");
    }
}
