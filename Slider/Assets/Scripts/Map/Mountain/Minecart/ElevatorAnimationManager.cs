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
       // topDispAnimator.Play("Disp Top Fade In");
        bottomDispAnimator.Play("Disp Bottom Fade In");
        //topDoorAnimator.Play("Open");
        bottomDoorAnimator.Play("Open");
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
        //dissolve indicator
        //topDispAnimator.Play("Disp Top Fade Out");
        bottomDispAnimator.Play("Disp Bottom Fade Out");
        yield return new WaitForSeconds(0.333f);

        //close doors
      //  topDoorAnimator.Play("Close");
        bottomDoorAnimator.Play("Close");

        //send in direction
       // topDispAnimator.Play(sendAnimName);
        bottomDispAnimator.Play(sendAnimName);

        //travel
        yield return new WaitForSeconds(2.833f);

        //open doors
      //  topDoorAnimator.Play("Open");
        bottomDoorAnimator.Play("Open");
      //  topDispAnimator.Play("Disp Top Fade In");
        bottomDispAnimator.Play("Disp Bottom Fade In");

        //yield return new WaitForSeconds(0.25f);
    }

    public void TestDoorOpen()
    {
        bottomDoorAnimator.Play("Open");

    }

    public void TestDoorClose()
    {
        bottomDoorAnimator.Play("Close");
    }
}
