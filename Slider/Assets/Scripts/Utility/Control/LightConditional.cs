using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LightConditional : MonoBehaviour
{
    public UnityEvent OnSuccess;
    public UnityEvent OnFailure;
    
    private LightManager lm;

    private void Awake() 
    {
        lm = GameObject.Find("LightManager")?.GetComponent<LightManager>();    
    }

    private void Update() 
    {
        if (CheckLit())
            OnSuccess.Invoke();
        else
            OnFailure.Invoke();
    }

    public bool CheckLit()
    {
        return (lm != null && lm.GetLightMaskAt((int)transform.position.x, (int)transform.position.y));
    }

    public void CheckLitSpec(Condition c)
    {
        c.SetSpec(CheckLit());
    }
}
