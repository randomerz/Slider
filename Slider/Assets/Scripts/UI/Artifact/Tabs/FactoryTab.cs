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
    [SerializeField] private Sprite blinkSprite;
    [SerializeField] private Image image;

    private Sprite queuedNextSprite;
    private bool blinking = false;

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

    public override void SetIsVisible(bool value)
    {
        isActive = !value ? false : gate.GateActive && !gate.Powered;
        isVisible = isActive;
        if (!value && gameObject.activeInHierarchy)
        {
            StartCoroutine(SetVisibleThenDisable());
        }
        else
        {
            gameObject.SetActive(isActive);
            UpdateVisibility();
        }
        if (gate.GateActive && !gate.Powered)
        {
            queuedNextSprite = gate.Powered ? successSprite : countdownSprite[gate.Countdown];
            image.sprite = queuedNextSprite;
        }
 
    }

    private void MoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        if (gate.GateActive && !gate.Powered)
        {
            if (gate.Countdown > 0)
            {
                queuedNextSprite = countdownSprite[gate.Countdown];
                StartCoroutine(BlinkThenShowNext());
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
                SetIsVisible(false);
            }
            image.sprite = queuedNextSprite;
        }
    }

    private IEnumerator BlinkThenShowNext(int numBlinks = 1)
    {
        if (!blinking)
        {
            Debug.Log("Entered Blink!");
            blinking = true;
            int currBlinks = numBlinks;
            while (currBlinks > 0)
            {
                image.sprite = blinkSprite;
                yield return new WaitForSeconds(0.25f);
                image.sprite = queuedNextSprite;
                currBlinks--;
                if (currBlinks > 0)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            image.sprite = queuedNextSprite;
            blinking = false;
        }
    }
}
