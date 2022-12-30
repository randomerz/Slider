using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator topDispAnimator;
    [SerializeField] private Animator topDoorAnimator;
    [SerializeField] private Animator bottomDispAnimator;
    [SerializeField] private Animator bottomDoorAnimator;

    private void Awake() {
        topDispAnimator.Play("Top");
        bottomDispAnimator.Play("Bottom");
        topDoorAnimator.Play("Open");
        bottomDoorAnimator.Play("Open");
    }

    public void SendUp(Minecart mc)
    {
        StartCoroutine(SendCoroutine(mc, "SendUp"));
    }

    public void SendDown(Minecart mc)
    {
        StartCoroutine(SendCoroutine(mc, "SendDown"));
    }


    private IEnumerator SendCoroutine(Minecart mc, string sendAnimName)
    {
        //dissolve indicator
        topDispAnimator.Play("Dissolve");
        bottomDispAnimator.Play("Dissolve");
        yield return new WaitForSeconds(0.25f);

        //close doors
        topDoorAnimator.Play("Close");
        bottomDoorAnimator.Play("Close");

        //send in direction
        topDispAnimator.Play(sendAnimName);
        bottomDispAnimator.Play(sendAnimName);

        //travel
        yield return new WaitForSeconds(1f);

        //open doors
        topDoorAnimator.Play("Open");
        bottomDoorAnimator.Play("Open");
        topDispAnimator.Play("Appear");
        bottomDispAnimator.Play("Appear");

        yield return new WaitForSeconds(0.25f);
        topDispAnimator.Play("Top");
        bottomDispAnimator.Play("Bottom");
    }
}
