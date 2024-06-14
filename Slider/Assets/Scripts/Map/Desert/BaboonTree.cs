using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaboonTree : MonoBehaviour
{
    private enum BaboonState
    {
        Disconnected,
        Connected,
        Snapping,
    }

    [SerializeField] private float baboonFallDuration;
    [SerializeField] private AnimationCurve baboonFallCurve;
    private bool isWalking;
    private bool isFalling;

    [Header("References")]
    [SerializeField] private Animator rightTreeAnimator;
    [SerializeField] private Animator leftTreeAnimator;
    [SerializeField] private RopeCoil ropeCoil;

    [SerializeField] private GameObject baboonGameObject;
    [SerializeField] private Animator baboonAnimator;
    [SerializeField] private SpriteRenderer baboonSpriteRenderer;
    [SerializeField] private Collider2D baboonCollider;
    [SerializeField] private Transform baboonFallTransform;

    private BaboonState state;
    private bool didHorizontal;
    private bool didVertical;
    private int numRopeConnections;
    private int numShakesAfterWalk;

    private bool checkedOnStileMoveThisFrame;

    private void Start() {
        if (SaveSystem.Current.GetBool("desertBaboonFinishedWalk") &&
           !SaveSystem.Current.GetBool("desertBaboonHasFallen"))
        {
            // bruh
            SaveSystem.Current.SetBool("desertBaboonHasFallen", true);
        }

        if (SaveSystem.Current.GetBool("desertBaboonHasFallen"))
        {
            BaboonOnGround();
        }
    }

    private void OnEnable() 
    {
        SGridAnimator.OnSTileMoveStart += OnSTileMove;
    }

    private void OnDisable() 
    {
        SGridAnimator.OnSTileMoveStart -= OnSTileMove;
    }

    private void Update() 
    {
        checkedOnStileMoveThisFrame = false;
    }

    public void ConnectRope()
    {
        if (state == BaboonState.Disconnected)
        {
            state = BaboonState.Connected;

            leftTreeAnimator.SetBool("bridgeActive", true);
            rightTreeAnimator.SetBool("bridgeActive", true);

            AudioManager.Play("Hat Click");

            ParticleManager.SpawnParticle(ParticleType.SmokePoof, ropeCoil.transform.position);
            ropeCoil.RemoveRopeFromPlayer();
            ropeCoil.SetRopeCoilActive(false);

            didHorizontal = didVertical = false;
            numRopeConnections += 1;
        }
    }

    public void DisconnectRope()
    {
        if (state == BaboonState.Connected)
        {
            state = BaboonState.Snapping;

            leftTreeAnimator.SetBool("bridgeActive", false);
            rightTreeAnimator.SetBool("bridgeActive", false);

            AudioManager.Play("Hurt");
        }
    }

    public void FinishSnapping()
    {
        if (state == BaboonState.Snapping)
        {
            state = BaboonState.Disconnected;

            ropeCoil.SetRopeCoilActive(true);
        }
    }

    private void OnSTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (state != BaboonState.Connected)
            return;

        // we don't want this being called 4 times per move
        if (checkedOnStileMoveThisFrame)
            return;
        checkedOnStileMoveThisFrame = true;

        SMove smove = e.smove;
        Vector2Int d2 = Vector2Int.zero; // the difference tile 2's move makes
        Vector2Int d3 = Vector2Int.zero;
        foreach (Movement m in smove.moves)
        {
            if (m.islandId == 2)
            {
                d2 = m.endLoc - m.startLoc;
            }
            else if (m.islandId == 3)
            {
                d3 = m.endLoc - m.startLoc;
            }
        }

        if (d2 != d3)
        {
            didHorizontal = didVertical = false;
            DisconnectRope();
        }
        else if (d2.x != 0)
        {
            didHorizontal = true;
        }
        else if (d2.y != 0)
        {
            didVertical = true;
        }

        SaveSystem.Current.SetString("desertRemainingShakeDirection",
            didHorizontal ? "vertically" : "horizontally"
        );

        if (isWalking && !isFalling)
        {
            if (d2 != d3)
            {
                BaboonFall();
            }
            else if (d2 != Vector2Int.zero)
            {
                AudioManager.Play("Baboon Screech");
                numShakesAfterWalk += 1;
            }
        }
    }


    // Baboon falling
    public void SetIsWalking(bool value)
    {
        isWalking = value;
    }

    public void BaboonFall()
    {
        isFalling = true;
        baboonGameObject.GetComponent<NPC>().TryEndCurrentWalk();
        StartCoroutine(AnimateBaboonFall());
    }

    private IEnumerator AnimateBaboonFall()
    {
        baboonGameObject.transform.SetParent(baboonFallTransform);
        baboonAnimator.Play("Falling");
        AudioManager.Play("Fall");

        Vector3 startPos = baboonGameObject.transform.position;
        float t = 0;
        while (t < baboonFallDuration)
        {
            float x = baboonFallCurve.Evaluate(t / baboonFallDuration);
            Vector3 newPos = Vector3.Lerp(startPos, baboonFallTransform.position, x);
            baboonGameObject.transform.position = newPos;

            yield return null;
            t += Time.deltaTime;
        }

        baboonGameObject.transform.position = baboonFallTransform.position;
        SaveSystem.Current.SetBool("desertBaboonHasFallen", true);

        // AudioManager.Play("Hurt");
        AudioManager.Play("Baboon Screech");

        BaboonOnGround();
    }

    public void BaboonOnGround()
    {
        baboonAnimator.Play("Fell");
        baboonSpriteRenderer.sortingOrder = 0;
        baboonCollider.enabled = true;
    }


    // NPC Conditionals
    public void IsStateDisconnected(Condition c)   => c.SetSpec(state == BaboonState.Disconnected);
    public void IsStateConnected(Condition c)      => c.SetSpec(state == BaboonState.Connected);
    public void IsStateSnapping(Condition c)       => c.SetSpec(state == BaboonState.Snapping);
    public void HasConnectedRopeOnce(Condition c)  => c.SetSpec(numRopeConnections >= 1);
    public void HasConnectedRopeTwice(Condition c) => c.SetSpec(numRopeConnections >= 2);
    public void IsOneDirectionDone(Condition c)    => c.SetSpec(didHorizontal || didVertical);
    public void IsBothDirectionsDone(Condition c)  => c.SetSpec(didHorizontal && didVertical); // yes is both directions done
    public void IsFalling(Condition c)             => c.SetSpec(isFalling);
    public void DidShakeAfterWalk(Condition c)     => c.SetSpec(numShakesAfterWalk >= 1);
    public void ThreeShakeAfterWalk(Condition c)     => c.SetSpec(numShakesAfterWalk >= 3);

}
