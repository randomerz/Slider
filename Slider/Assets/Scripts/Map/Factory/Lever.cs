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
    protected bool _targetVisualOn;

    public bool debugLog;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;

        _animator = GetComponent<Animator>();
        _pConds = GetComponent<PlayerConditionals>();
    }

    private void Start() 
    {
        if (powerOnStart && !shouldSaveLeverState) 
        {
            _targetVisualOn = true;     
            SetState(true);
        }

        if (shouldSaveLeverState)
        {
            bool shouldBePowered = SaveSystem.Current.GetBool(saveLeverString, powerOnStart);
            _targetVisualOn = shouldBePowered;
            _animator.SetBool("isOn", shouldBePowered);
            //_animator.SetTrigger("Switched");
            if (shouldBePowered)
            {
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
        if (ShouldFlip())
        {
            SwitchVisuals();
        } 
    }

    private bool ShouldFlip()
    {
        if (_isAnimating)
        {
            return false;
        }
        return (_targetVisualOn != _isPowerSource && !invertSignal) ||
               (_targetVisualOn != _isPowerSource && invertSignal);
    }

    private void HandleBlackoutStarted()
    {
        _pConds.DisableConditionals();
        _targetVisualOn = false;
        SetState(false);
    }

    private void HandleBlackoutEnded()
    {
        _pConds.EnableConditionals();
    }


    public virtual void Switch()
    {
        AudioManager.Play("UI Click");

        _targetVisualOn = !_targetVisualOn;
    }

    private void SwitchVisuals()
    {
        SetState(!PoweredConditionsMet());
    }

    public void SetState(bool value) {

        bool powerConditionsMet = PoweredConditionsMet();
        if (invertSignal)
        {
            value = !value;
            powerConditionsMet = !powerConditionsMet;
        }

        if (powerConditionsMet == value) {
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
        _animator.SetBool("isOn", true);

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return (state.IsName("Turning On") || state.IsName("On")) && state.normalizedTime > 0.6f;
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
        _animator.SetBool("isOn", false);
        
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return (state.IsName("Turning Off") || state.IsName("Off")) && state.normalizedTime > 0.4f;
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