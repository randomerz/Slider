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

    private bool gateActive;    //Whether the timed gate is activated and displaying the countdown (Note: This is different from Powered)
    [SerializeField] private int countdown;

    private Coroutine waitingToEndGate;
    private bool blinking;  //Ensures that only one blink coroutine is executing at a time.

    public bool GateActive => gateActive;

    public override bool Powered
    {
        get
        {
            bool normal = inputsPowered != null && inputsPowered.Count >= numInputs;
            return invertSignal ? !normal : normal;
        }
    }

    #region Unity Callbacks
    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        inputsPowered = new HashSet<ElectricalNode>();
        waitingToEndGate = null;
        queuedNextSprite = waitingSprite;
        sr.sprite = waitingSprite;

        if (numTurns > 5)
        {
            Debug.LogError("Countdowns greater than 5 are not supported");
        }

        blinking = false;
    }

    private new void OnEnable()
    {
        base.OnEnable();
        UIArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        UIArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
    }
    #endregion

    #region ElectricalNode Overrides

    protected override void PropagateSignal(bool value, ElectricalNode prev, HashSet<ElectricalNode> recStack, int numRefs = 1)
    {
        bool oldPowered = Powered;
        if (EvaluateNodeInput(value, prev, recStack, numRefs) && value && gateActive)
        {
            inputsPowered.Add(prev);

            if (Powered != oldPowered)
            {
                OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
            }
        }
    }
    public override void OnPoweredHandler(OnPoweredArgs e)
    {

        base.OnPoweredHandler(e);
        if (e.powered)
        {
            //Once the timed gate is powered, it essentially acts as an input source to it's outputs, so we only care about when it is powered.
            PushSignalToOutput(true, new HashSet<ElectricalNode>(), 1);
            EvaluateGate();
        }
    }
    #endregion


    //This is called via player interaction
    public void ActivateGate()
    {
        if (!Powered)
        {
            gateActive = true;
            countdown = numTurns;
            queuedNextSprite = countdownSprite[numTurns];

            bool oldPowered = Powered;
            foreach (ElectricalNode input in powerPathPrevs)
            {
                //Add all the nodes that were already connected to the gate when it was turned on.
                inputsPowered.Add(input);
            }
            if (Powered != oldPowered)
            {
                OnPowered?.Invoke(new OnPoweredArgs { powered = Powered });
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
            gateActive = false;
            inputsPowered.Clear();
            OnGateDeactivated?.Invoke();
        }

        queuedNextSprite = Powered ? successSprite : failureSprite;
        StartCoroutine(BlinkThenShowNext());
    }

    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (gateActive && !Powered)
        {
            countdown--;

            if (countdown > 0)
            {
                queuedNextSprite = countdownSprite[countdown];
                StartCoroutine(BlinkThenShowNext());
            } else if (countdown == 0)
            {
                queuedNextSprite = countdownSprite[countdown];
                StartCoroutine(BlinkUntilNextSpriteChange());
                waitingToEndGate = StartCoroutine(WaitAfterMove(EvaluateGate));
            } else if (countdown < 0)
            {
                //If player tries to queue another move, just stop the gate immediately. (avoids some nasty edge cases)
                if (waitingToEndGate != null)
                {
                    StopCoroutine(waitingToEndGate);
                }

                EvaluateGate();
            }
        }
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
        if (!blinking)
        {
            blinking = true;
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
            blinking = false;
        }
    }

    private IEnumerator BlinkUntilNextSpriteChange()
    {
        if (!blinking)
        {
            blinking = true;
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
            blinking = false;
        }
    }
}