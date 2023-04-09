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

    [Header("References")]
    [SerializeField] private Animator rightTreeAnimator;
    [SerializeField] private Animator leftTreeAnimator;
    [SerializeField] private RopeCoil ropeCoil;

    private BaboonState state;

    private void OnEnable() 
    {
        SGridAnimator.OnSTileMoveStart += OnSTileMove;
    }

    private void OnDisable() 
    {
        SGridAnimator.OnSTileMoveStart -= OnSTileMove;
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
        SMove move = e.smove;
        DisconnectRope();
    }
}
