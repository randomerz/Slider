using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimRedirect : MonoBehaviour
{
    [SerializeField] private GemMachine gemMachine;
    public WaterWheel waterWheel;

    public void OnEndAbsorb()
    {
        gemMachine.OnEndAbsorb();
    }

    public void OnEndHeaterFill()
    {
        waterWheel.OnFillHeaterEnd();
    }

    public void OnEndAbsorbLava()
    {
        waterWheel.OnEndAbsorbLava();
    }
}
