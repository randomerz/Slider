using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryTab : ArtifactTab
{
    [SerializeField] private TimedGate gate;
    [SerializeField] private List<Sprite> countdownSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;
    [SerializeField] private Image image;

    private bool shownComplete;
    private Sprite queuedNextSprite;

    public bool Activated { get { return gate.GateActive; } }

    private void OnEnable()
    {
        UIArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
        queuedNextSprite = countdownSprite[gate.Countdown];
    }

    private void OnDisable()
    {
        UIArtifact.MoveMadeOnArtifact -= MoveMadeOnArtifact;
    }

    public void SetIsVisible()
    {
        if (gate.GateActive && (!gate.Powered || !shownComplete))
        {
            queuedNextSprite = gate.Powered ? successSprite : countdownSprite[gate.Countdown];
            shownComplete = gate.Powered;
            image.sprite = queuedNextSprite;
            base.SetIsVisible(true);
        }
        else base.SetIsVisible(false);

    }

    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (gate.GateActive && !gate.Powered)
        {
            if (gate.Countdown > 0)
            {
                queuedNextSprite = countdownSprite[gate.Countdown];
                //Insert Cosmetic coroutine to advance image
            }
            else if (gate.Countdown == 0)
            {
                queuedNextSprite = countdownSprite[gate.Countdown]; //set to 0
                                                                    //StartCoroutine(BlinkUntilNextSpriteChange());
                                                                    //waitingToEndGate = StartCoroutine(WaitAfterMove(EvaluateGate));
                queuedNextSprite = gate.Powered ? successSprite : failureSprite;
            }
            else if (gate.Countdown < 0)
            {
                //If player tries to queue another move, just stop the gate immediately. (avoids some nasty edge cases)
                /*
                if (waitingToEndGate != null)
                {
                    StopCoroutine(waitingToEndGate);
                }

                EvaluateGate();
                */
                queuedNextSprite = gate.Powered ? successSprite : failureSprite;
            }
            Debug.Log(image);
            Debug.Log(queuedNextSprite);
            image.sprite = queuedNextSprite;
        }
    }
}
