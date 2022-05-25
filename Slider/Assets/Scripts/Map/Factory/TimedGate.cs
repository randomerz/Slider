using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedGate : ElectricalNode
{
    [SerializeField] private int numTurns;
    [SerializeField] private int numInputs;

    //[SerializeField] private Animator anim;

    //All the various things the gate can display
    [SerializeField] private Sprite[] countdownSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;
    [SerializeField] private Sprite waitingSprite;
    [SerializeField] private Sprite blinkSprite;
    [SerializeField] private SpriteRenderer sr;

    private Sprite nextSprite;  //The next sprite to show (queue up) after a blink

    private HashSet<ElectricalNode> inputsPowered;

    private bool gateActive;    //Whether the timed gate is activated and displaying the countdown (Note: This is different from Powered)
    [SerializeField] private int countdown;

    private Coroutine waitingToEndGate;
    private bool blinking;  //Ensures that only one blink coroutine is executing at a time.

    public UnityEvent OnGateActivated;
    public UnityEvent OnGateDeactivated;

    public bool GateActive => gateActive;

    public override bool Powered
    {
        get
        {
            bool normal = inputsPowered != null && inputsPowered.Count >= numInputs;
            return invertSignal ? !normal : normal;
        }
    }

    #region Unity Events
    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        inputsPowered = new HashSet<ElectricalNode>();
        waitingToEndGate = null;
        nextSprite = waitingSprite;
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
        UIArtifact.MoveMadeOnArtifact += OnMoveMade;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        UIArtifact.MoveMadeOnArtifact -= OnMoveMade;
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
        //Once the timed gate is powered, it essentially acts as an input source to it's outputs, so we only care about when it is powered.

        if (e.powered)
        {
            PushSignalToOutput(true, new HashSet<ElectricalNode>(), 1);
            EvaluateGate();
            StartCoroutine(BlinkThenShowNext());
        }
    }
    #endregion

    private void OnMoveMade(object sender, System.EventArgs e)
    {
        if (gateActive && !Powered)
        {
            countdown--;

            if (countdown > 0)
            {
                nextSprite = countdownSprite[countdown];
                StartCoroutine(BlinkThenShowNext());
            } else if (countdown == 0)
            {
                nextSprite = countdownSprite[countdown];
                StartCoroutine(BlinkUntilNextSpriteChange());
                waitingToEndGate = StartCoroutine(WaitToEvaluateGate());
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

    private IEnumerator WaitToEvaluateGate()
    {
        bool tilesAreMoving = true;
        while (tilesAreMoving)
        {
            tilesAreMoving = SGrid.current.TilesMoving();

            if (!tilesAreMoving)
            {
                //Wait a bit, and then check again. If there's still no movement, then we can safely turn off the gate.
                //This gives the player enough leeway to complete the puzzle at the last second (i.e. puzzle 3c)
                yield return new WaitForSeconds(0.4f);

                tilesAreMoving = SGrid.current.TilesMoving();
            } else
            {
                yield return null;  //Resume doing other stuff, or else this will spinlock.
            }
        }
        EvaluateGate();
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
                sr.sprite = nextSprite;
                currBlinks--;
                if (currBlinks > 0)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            sr.sprite = nextSprite;
            blinking = false;
        }
    }

    private IEnumerator BlinkUntilNextSpriteChange()
    {
        if (!blinking)
        {
            blinking = true;
            Sprite currSprite = nextSprite;
            while (nextSprite == currSprite)
            {
                sr.sprite = blinkSprite;
                yield return new WaitForSeconds(0.25f);
                sr.sprite = nextSprite;
                if (currSprite == nextSprite)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }
            sr.sprite = nextSprite;
            blinking = false;
        }
    }

    //This is called via player interaction
    public void ActivateGate()
    {
        //You can restart the gate in the middle, but once it succeeds you can't restart it.
        if (!Powered)
        {
            gateActive = true;
            countdown = numTurns;
            nextSprite = countdownSprite[numTurns];

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
        nextSprite = Powered ? successSprite : failureSprite;
        StartCoroutine(BlinkThenShowNext());

        if (!Powered)
        {
            //Player failed to power the inputs in time.
            gateActive = false;
            inputsPowered.Clear();
            OnGateDeactivated?.Invoke();
        }

    }
}