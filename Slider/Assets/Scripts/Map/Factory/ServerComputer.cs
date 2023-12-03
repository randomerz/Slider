using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ServerComputer : MonoBehaviour
{
    [SerializeField] private ElectricalNode power;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerConditionals pConds;
    [SerializeField] private bool isDecoration;

    public UnityEvent OnInteract;

    private void Awake()
    {
        pConds?.DisableConditionals();
    }

    private void Start() 
    {
        if (isDecoration)
        {
            TurnOn();
        }
    }

    private void OnEnable()
    {
        anim.SetBool("Powered", power.Powered);
    }

    #region Called By Events
    public void TurnOn()
    {
        // Debug.Log("Powered Computer On");
        anim.SetBool("Powered", true);
        pConds?.EnableConditionals();
    }

    public void TurnOff()
    {
        // Debug.Log("Powered Computer Off");
        anim.SetBool("Powered", false);
        pConds?.DisableConditionals();
    }

    public void OnPlayerInteract()
    {
        OnInteract?.Invoke();
    }
    #endregion
}
