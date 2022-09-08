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
    private Coroutine waitingToEndGate;
    private bool blinking = false;

    public bool Activated { get { return gate.GateActive; } }

    private void OnEnable()
    {
        UIArtifact.MoveMadeOnArtifact += MoveMadeOnArtifact;
        if (gate.Countdown <= 0) gameObject.SetActive(false);
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
        // Debug.Log("MoveMadeOnArtifact");
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
                StartCoroutine(BlinkUntilNextSpriteChange());
                waitingToEndGate = StartCoroutine(WaitAfterMove());
            }
            else if (gate.Countdown < 0)
            {
                if (waitingToEndGate != null) StopCoroutine(WaitAfterMove());
                SetIsVisible(false);
            }
        }
    }

    private IEnumerator BlinkThenShowNext(int numBlinks = 1)
    {
        if (!blinking)
        {
            blinking = true;
            int currBlinks = numBlinks;
            while (currBlinks > 0)
            {
                // Debug.Log("num blinks: " + currBlinks);
                image.sprite = blinkSprite;
                yield return new WaitForSeconds(0.5f);
                image.sprite = queuedNextSprite;
                currBlinks--;
                if (currBlinks > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }

            image.sprite = queuedNextSprite;
            blinking = false;
        }
    }
    private IEnumerator WaitAfterMove()
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
            }
            else
            {
                yield return null;
            }
        }
        queuedNextSprite = gate.Powered ? successSprite : failureSprite;
        StartCoroutine(WaitBeforeDisabling());
    }
    private IEnumerator BlinkUntilNextSpriteChange()
    {
        if (!blinking)
        {
            blinking = true;
            Sprite currSprite = queuedNextSprite;
            while (queuedNextSprite == currSprite)
            {
                image.sprite = blinkSprite;
                yield return new WaitForSeconds(0.25f);
                image.sprite = queuedNextSprite;
                if (currSprite == queuedNextSprite)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }
            image.sprite = queuedNextSprite;
            blinking = false;
        }
    }

    private IEnumerator WaitBeforeDisabling()
    {
        yield return new WaitForSeconds(1f);
        SetIsVisible(false);
    }
}
