using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator topDispAnimator;
    [SerializeField] private Animator topDoorAnimator;
    [SerializeField] private Animator bottomDispAnimator;
    [SerializeField] private Animator bottomDoorAnimator;

    private bool isAnimating = false;
    private bool repaired = false;

    private void Start() {
        topDispAnimator.Play("Disp Top Fade In");
        bottomDispAnimator.Play("Disp Bottom Fade In");
        if(repaired)
            OpenDoors();
        else
            CloseDoors();
    }

    public void SendUp()
    {
        StartCoroutine(SendCoroutine("Disp Send Up"));
    }

    public void SendDown()
    {
        StartCoroutine(SendCoroutine("Disp Send Down"));
    }

    private IEnumerator SendCoroutine(string sendAnimName)
    {
        isAnimating = true;
        
        topDispAnimator.Play("Disp Top Fade Out");
        bottomDispAnimator.Play("Disp Bottom Fade Out");
        yield return new WaitForSeconds(0.333f);

        CloseDoors();
        topDispAnimator.Play(sendAnimName);
        bottomDispAnimator.Play(sendAnimName);
        yield return new WaitForSeconds(2.833f);

        OpenDoors();
        topDispAnimator.Play("Disp Top Fade In");
        bottomDispAnimator.Play("Disp Bottom Fade In");

        yield return new WaitForSeconds(1f);

        isAnimating = false;
    }

    private void CloseDoors()
    {
        topDoorAnimator.Play("Close");
        bottomDoorAnimator.Play("Close");
        //update colliders
    }

    private void OpenDoors()
    {
        topDoorAnimator.Play("Open");
        bottomDoorAnimator.Play("Open");
        //update colliders
    }

    public void Repair()
    {
        repaired = true;
        OpenDoors();
        //delete extra sprites or whatnot
    }
}
