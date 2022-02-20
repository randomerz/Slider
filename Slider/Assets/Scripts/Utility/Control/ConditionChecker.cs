using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionChecker : MonoBehaviour
{
    public Conditionals conditionals;

    public void CheckConditions()
    {
        conditionals.CheckConditions();
    }

    public void PlayErrorSound()
    {
        AudioManager.Play("Artifact Error");
    }

    public void DebugLogString(string s)
    {
        Debug.Log(s);
    }
}
