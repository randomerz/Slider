using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    private Sprite queuedNextSprite;

    private HashSet<ElectricalNode> inputsPowered;

    private bool _gateActive;    //Whether the timed gate is activated and displaying the countdown (Note: This is different from Powered)
    private int _countdown;

    private Coroutine _waitToEndGateCoroutine;
    private bool _blinking;  //Ensures that only one blink coroutine is executing at a time.

    public bool GateActive => _gateActive;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        inputsPowered = new HashSet<ElectricalNode>();
        _waitToEndGateCoroutine = null;
        queuedNextSprite = waitingSprite;
        sr.sprite = waitingSprite;

        if (numTurns > 5)
        {
            Debug.LogError("Countdowns greater than 5 are not supported");
        }

        _blinking = false;
    }

    private new void OnEnable()
    {
        base.OnEnable();

        if (Powered)
        {
            EvaluateGate();
        }
        UIArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        _blinking = false;   //L: Coroutines are stopped when game objects are disabled.
        UIArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
    }

    private void OnValidate()
    {
        if (numTurns > 4)
        {
            Debug.LogError("Timed Gate only supports countdowns of 4 or less");
        }
    }

    #region ElectricalNode Overrides

    protected override bool PoweredConditionsMet()
    {
        bool allInputsPowered = inputsPowered != null && inputsPowered.Count >= numInputs;
        return (invertSignal ? !allInputsPowered : allInputsPowered);
    }

    protected override void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        if (EvaluateNodeInput(value, prev, recStack, numRefs) && value && _gateActive)
        {
            PowerInput(prev);
        }
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {

        base.OnPoweredHandler(e);
        if (e.powered)
        {
            //Once the timed gate is powered, it essentially acts as an input source to it's outputs. It will not turn off
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
                // "Reseting" the gate
                inputsPowered.Clear();
                OnGateDeactivated?.Invoke();
            }
            
            _gateActive = true;
            _countdown = numTurns;
            queuedNextSprite = countdownSprite[numTurns];

            foreach (ElectricalNode input in powerPathPrevs.Keys)
            {
                //Add all the nodes that were already connected to the gate when it was turned on.
                PowerInput(input);
            }

            StartCoroutine(BlinkThenShowNext());
            OnGateActivated?.Invoke();
        }
    }

    public void EvaluateGate()
    {
        if (!Powered)
        {
            //Player failed to power the inputs in time.
            _gateActive = false;
            inputsPowered.Clear();
            OnGateDeactivated?.Invoke();
        }

        queuedNextSprite = Powered ? successSprite : failureSprite;
        StartCoroutine(BlinkThenShowNext());
    }

    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (_gateActive && !Powered)
        {
            _countdown--;

            if (_countdown > 0)
            {
                queuedNextSprite = countdownSprite[_countdown];
                StartCoroutine(BlinkThenShowNext());
            } else if (_countdown == 0)
            {
                queuedNextSprite = countdownSprite[_countdown];
                StartCoroutine(BlinkUntilNextSpriteChange());
                _waitToEndGateCoroutine = StartCoroutine(WaitAfterMove(EvaluateGate));
            } else if (_countdown < 0)
            {
                //If player tries to queue another move, just stop the gate immediately. (avoids some nasty edge cases)
                if (_waitToEndGateCoroutine != null)
                {
                    StopCoroutine(_waitToEndGateCoroutine);
                }

                EvaluateGate();
            }
        }
    }

    private void PowerGate()
    {
        PushSignalToOutput(true, new HashSet<ElectricalNode>(), 1);
        EvaluateGate();

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
                sr.sprite = queuedNextSprite;
                currBlinks--;
                if (currBlinks > 0)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            sr.sprite = queuedNextSprite;
            _blinking = false;
        }
    }

    private IEnumerator BlinkUntilNextSpriteChange()
    {
        if (!_blinking)
        {
            _blinking = true;
            Sprite currSprite = queuedNextSprite;
            while (queuedNextSprite == currSprite)
            {
                sr.sprite = blinkSprite;
                yield return new WaitForSeconds(0.25f);
                sr.sprite = queuedNextSprite;
                if (currSprite == queuedNextSprite)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }
            sr.sprite = queuedNextSprite;
            _blinking = false;
        }
    }

    public void PowerInput(ElectricalNode prev)
    {
        inputsPowered.Add(prev);
    }
}