using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Note: Gate has 3 states: Deactivated, Activated, and Powered
//Deactivated: !_gateActive && !Powered
//Activated: _gateActive && !Powered
//Powered: Powered
public class TimedGate : ElectricalNode
{
    [Header("Timed Gate")]

    [SerializeField] private int numTurns;
    [SerializeField] private int numInputs;
    [SerializeField] private Sprite[] countdownSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;
    [SerializeField] private Sprite waitingSprite;
    [SerializeField] private Sprite blinkSprite;
    [SerializeField] private SpriteRenderer sr;

    public UnityEvent OnGateActivated;
    public UnityEvent OnGateDeactivated;

    private Sprite _queuedNextSprite;
    private HashSet<ElectricalNode> _inputsPowered;
    private bool _gateActive;    //Whether the timed gate is activated and displaying the countdown
    private int _countdown;
    private Coroutine _waitToEndGateCoroutine;
    private bool _blinking;  //Ensures that only one blink coroutine is executing at a time.

    private PlayerConditionals _pConds;

    public bool GateActive => _gateActive;

    private const string PROGRESS_SOUND_PREFIX = "FactoryTimedGateProgress";

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        _inputsPowered = new HashSet<ElectricalNode>();
        _waitToEndGateCoroutine = null;
        _queuedNextSprite = waitingSprite;
        _blinking = false;

        _pConds = GetComponent<PlayerConditionals>();

        sr.sprite = waitingSprite;
    }

    private new void OnEnable()
    {
        base.OnEnable();

        UIArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
        PowerCrystal.blackoutStarted += HandleBlackoutStarted;
        PowerCrystal.blackoutEnded += HandleBlackoutEnded;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        _blinking = false;   //L: Coroutines are stopped when game objects are disabled.
        UIArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
        PowerCrystal.blackoutStarted -= HandleBlackoutStarted;
        PowerCrystal.blackoutEnded -= HandleBlackoutEnded;
    }

    private void OnValidate()
    {
        if (numTurns > 4)
        {
            Debug.LogError("Timed Gate only supports countdowns of 4 or less");
        }

        affectedByBlackout = false; //Timed gates are not affected!
    }

    private void HandleBlackoutStarted()
    {
        //L: Might as well do this. Player shouldn't be messing with the gates anyway.
        _pConds.DisableConditionals();
    }

    private void HandleBlackoutEnded()
    {
        _pConds.EnableConditionals();
    }

    #region ElectricalNode Overrides

    protected override bool PoweredConditionsMet()
    {
        bool allInputsPowered = _inputsPowered != null && _inputsPowered.Count >= numInputs;
        return (invertSignal ? !allInputsPowered : allInputsPowered);
    }

    protected override void EvaluateNodeDuringPropagate(bool value, ElectricalNode prev)
    {
        //if (EvaluateNodeInput(value, prev) && value && _gateActive)
        if (value && _gateActive && prev != null && prev.Powered)
        {
            AddInputPowered(prev);
        }
    }

    private void AddInputPowered(ElectricalNode node)
    {
        int prevCount = _inputsPowered.Count;
        _inputsPowered.Add(node);
        int newCount = _inputsPowered.Count;

        if (newCount > prevCount)
        {
            AudioManager.Play($"{PROGRESS_SOUND_PREFIX}{prevCount + 1}");
        }
    }

    protected override bool ShouldPushOutgoingOntoStack(bool value, ElectricalNode prev)
    {
        return prev == null;    //Only if we started here aka powered the gate.
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {

        base.OnPoweredHandler(e);
        if (e.powered)
        {
            PowerGate();
        }
    }
    #endregion

    public int GetTabSpriteIndex()
    {
        for (int i = 0; i < countdownSprite.Length; i++)
        {
            if (sr.sprite == countdownSprite[i])
            {
                return i;
            }
        }

        if (sr.sprite == successSprite)
        {
            return countdownSprite.Length;
        }
        if (sr.sprite == failureSprite)
        {
            return countdownSprite.Length+1;
        }
        if (sr.sprite == blinkSprite)
        {
            return countdownSprite.Length+2;
        }

        return -1;
    }

    //This is called via player interaction
    public void ActivateGate()
    {
        if (!Powered)
        {
            if (_gateActive)
            {
                // "Resetting" the gate
                _inputsPowered.Clear();
                OnGateDeactivated?.Invoke();
            }

            AudioManager.Play("UI Click");
            
            _gateActive = true;
            _countdown = numTurns;
            _queuedNextSprite = countdownSprite[numTurns];

            foreach (ElectricalNode input in _incomingNodes)
            {
                //Add all the nodes that were already connected to the gate when it was turned on.
                if (input.Powered)
                {
                    AddInputPowered(input);
                    //input.StartSignal(true); // turn on diodes
                }
            }

            StartCoroutine(BlinkThenShowNext());
            OnGateActivated?.Invoke();
        }
    }

    public void HardRestartGate()
    {
        if (Powered)
        {
            StartSignal(false);
            _isPowered = false;
            UpdateDFS();
        }
        
        _gateActive = false;
        _inputsPowered.Clear();
        OnGateDeactivated?.Invoke();
        _queuedNextSprite = waitingSprite;
        StartCoroutine(BlinkThenShowNext());
    }

    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (_gateActive && !Powered)
        {
            _countdown--;

            if (_countdown > 0)
            {
                _queuedNextSprite = countdownSprite[_countdown];
                StartCoroutine(BlinkThenShowNext());
            }
            else if (_countdown == 0)
            {
                _queuedNextSprite = countdownSprite[_countdown];
                StartCoroutine(BlinkUntilNextSpriteChange());
                _waitToEndGateCoroutine = StartCoroutine(WaitAfterMove(CheckFailedToPower));
            }
            else if (_countdown < 0)
            {
                //If player tries to queue another move, just stop the gate immediately. (avoids some nasty edge cases)
                if (_waitToEndGateCoroutine != null)
                {
                    StopCoroutine(_waitToEndGateCoroutine);
                }

                CheckFailedToPower();
            }
        }
    }

    private void CheckFailedToPower()
    {
        if (!Powered)
        {
            //Player failed to power the inputs in time.
            
            AudioManager.Play("Artifact Error");

            _gateActive = false;
            _inputsPowered.Clear();

            _queuedNextSprite = failureSprite;
            StartCoroutine(BlinkThenShowNext());
            OnGateDeactivated?.Invoke();
        }
    }

    private void PowerGate()
    {
        StartSignal(true);
        _queuedNextSprite = successSprite;
        StartCoroutine(BlinkThenShowNext());
        AudioManager.Play("Puzzle Complete");
    }

    private IEnumerator WaitAfterMove(System.Action callback)
    {
        bool tilesAreMoving = true;
        while (tilesAreMoving)
        {
            tilesAreMoving = SGrid.Current.TilesMoving();

            if (!tilesAreMoving)
            {
                //Wait a bit, and then check again. If there's still no movement, then we can safely turn off the gate.
                //This gives the player enough leeway to complete the puzzle at the last second (i.e. puzzle 3c)
                yield return new WaitForSeconds(0.4f);

                tilesAreMoving = SGrid.Current.TilesMoving();
            } else
            {
                yield return null;
            }
        }

        callback();
    }

    private IEnumerator BlinkThenShowNext(int numBlinks = 1)
    {
        if (!_blinking)
        {
            _blinking = true;
            int currBlinks = numBlinks;
            while (currBlinks > 0)
            {
                sr.sprite = blinkSprite;
                yield return new WaitForSeconds(0.25f);
                sr.sprite = _queuedNextSprite;
                currBlinks--;
                if (currBlinks > 0)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            sr.sprite = _queuedNextSprite;
            _blinking = false;
        }
    }

    private IEnumerator BlinkUntilNextSpriteChange()
    {
        if (!_blinking)
        {
            _blinking = true;
            Sprite currSprite = _queuedNextSprite;
            while (_queuedNextSprite == currSprite)
            {
                sr.sprite = blinkSprite;
                yield return new WaitForSeconds(0.25f);
                sr.sprite = _queuedNextSprite;
                if (currSprite == _queuedNextSprite)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }
            sr.sprite = _queuedNextSprite;
            _blinking = false;
        }
    }
}