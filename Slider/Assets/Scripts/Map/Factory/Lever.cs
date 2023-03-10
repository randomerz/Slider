using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ElectricalNode
{

    [SerializeField] private bool shouldSaveLeverState; // also stay powered no matter what
    public string saveLeverString;

    private Animator _animator;
    private PlayerConditionals _pConds;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;

        _animator = GetComponent<Animator>();
        _pConds = GetComponent<PlayerConditionals>();
    }

    private void Start() 
    {
        if (powerOnStart) SetState(true);

        if (shouldSaveLeverState)
        {
            if (SaveSystem.Current.GetBool(saveLeverString))
            {
                _animator.SetTrigger("Switched");
                StartSignal(true);
            }
        }
    }

    private new void OnEnable()
    {
        base.OnEnable();
        PowerCrystal.blackoutStarted += HandleBlackoutStarted;
        PowerCrystal.blackoutEnded += HandleBlackoutEnded;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        PowerCrystal.blackoutStarted -= HandleBlackoutStarted;
        PowerCrystal.blackoutEnded -= HandleBlackoutEnded;
    }

    private void HandleBlackoutStarted()
    {
        _pConds.DisableConditionals();
        SetState(false);
    }

    private void HandleBlackoutEnded()
    {
        _pConds.EnableConditionals();
    }

    public void Switch()
    {
        SetState(!PoweredConditionsMet());
    }

    public void SetState(bool value) {
        if (PoweredConditionsMet() == value) {
            return;
        }

        if (value) {
            StartCoroutine(TurnOn());
        } else {
            StartCoroutine(TurnOff());
        }
    }

    public IEnumerator TurnOn() {
        _animator.SetTrigger("Switched");
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Turning On") && state.normalizedTime > 0.6f;
        });

        StartSignal(true);

        if (shouldSaveLeverState) // stay powered
        {
            SaveSystem.Current.SetBool(saveLeverString, true);
        }
    }

    public IEnumerator TurnOff() {
        _animator.SetTrigger("Switched");
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Turning Off") && state.normalizedTime > 0.4f;
        });

        StartSignal(false);
    }
}