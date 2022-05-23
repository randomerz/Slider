using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedGate : ElectricalNode
{
    [SerializeField] private int numTurns;
    [SerializeField] private int numInputs;

    [SerializeField] private Animator anim;

    private HashSet<ElectricalNode> inputsPowered;

    private bool gateActive;    //Whether the timed gate is activated and displaying the countdown (Note: This is different from Powered)
    [SerializeField] private int countdown;

    private Coroutine waitingToEndGate;

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
        anim.SetBool("Active", false);
        anim.SetBool("Powered", false);

        inputsPowered = new HashSet<ElectricalNode>();
        waitingToEndGate = null;
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
            anim.SetBool("Powered", true);
            PushSignalToOutput(true, new HashSet<ElectricalNode>(), 1);
        }
    }
    #endregion

    private void OnMoveMade(object sender, System.EventArgs e)
    {
        if (gateActive)
        {
            countdown--;

            if (countdown == 0)
            {
                waitingToEndGate = StartCoroutine(WaitToTurnGateOff());
            } else if (countdown < 0)
            {
                //If player tries to queue another move, just stop the gate immediately. (avoids some nasty edge cases)
                if (waitingToEndGate != null)
                {
                    StopCoroutine(waitingToEndGate);
                }

                GateOff();
            }
        }
    }

    private IEnumerator WaitToTurnGateOff()
    {
        bool tilesAreMoving = true;
        while (tilesAreMoving)
        {
            tilesAreMoving = SGrid.current.TilesMoving();

            if (!tilesAreMoving)
            {
                //Wait a bit, and then check again. If there's still no movement, then we can safely turn off the gate.
                //This gives the player enough leeway to complete the puzzle at the last second (i.e. puzzle 3c)
                yield return new WaitForSeconds(0.2f);

                tilesAreMoving = SGrid.current.TilesMoving();
            } else
            {
                yield return null;  //Resume doing other stuff, or else this will spinlock.
            }
        }
        GateOff();
    }

    public void GateOn()
    {
        gateActive = true;
        anim.SetBool("Active", true);
        countdown = numTurns;

        foreach (ElectricalNode input in powerPathPrevs)
        {
            //Add all the nodes that were already connected to the gate when it was turned on.
            inputsPowered.Add(input);
        }
    }

    public void GateOff()
    {
        gateActive = false;
        anim.SetBool("Active", false);

        if (!Powered)
        {
            //Player failed to power the inputs in time.
            inputsPowered.Clear();
        }
    }
}