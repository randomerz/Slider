using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ElectricalNode
{

    [SerializeField] private bool shouldSaveLeverState; // also stay powered no matter what
    public string saveLeverString;

    private Animator _animator;
    private PlayerConditionals _pConds;

    private bool _isAnimating;
    private bool _targetVisualOn;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;

        _animator = GetComponent<Animator>();
        _pConds = GetComponent<PlayerConditionals>();
    }

    private void Start() 
    {
        if (powerOnStart) 
        {
            _targetVisualOn = true;     
            SetState(true);
        }

        if (shouldSaveLeverState)
        {
            if (SaveSystem.Current.GetBool(saveLeverString))
            {
                _targetVisualOn = true;
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

    protected override void Update() {
        base.Update();
        if(!_isAnimating && _targetVisualOn != _isPowerSource)
        {
            //Switch();
            SwitchVisuals();
        } 
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
        AudioManager.Play("UI Click");

        _targetVisualOn = !_targetVisualOn;

        //SetState(!PoweredConditionsMet());
    }

    private void SwitchVisuals()
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
        _isAnimating = true;
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

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("On");
        });
        _isAnimating = false;
    }

    public IEnumerator TurnOff() {
        _isAnimating = true;
        _animator.SetTrigger("Switched");
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Turning Off") && state.normalizedTime > 0.4f;
        });

        StartSignal(false);

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("Off");
        });
        _isAnimating = false;
    }
}