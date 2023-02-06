using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimRedirect : MonoBehaviour
{
    [SerializeField] private GemMachine gemMachine;

    public void OnEndAbsorb()
    {
        gemMachine.OnEndAbsorb();
    }
}
