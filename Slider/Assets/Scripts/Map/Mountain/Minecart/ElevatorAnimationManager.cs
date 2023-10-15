using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator topDispAnimator;
    [SerializeField] private Animator topDoorAnimator;
    [SerializeField] private Animator bottomDispAnimator;
    [SerializeField] private Animator bottomDoorAnimator;

    public List<GameObject> deactivateOnFix;
    public List<GameObject> brokenObj;
    public GameObject bottompos;

   // private bool isAnimating = false;
    private bool isBroken = false;
    //private bool repaired = false;
    public MinecartElevator minecartElevator;

    private void Start() {
        topDispAnimator.Play("Disp Top Fade In");
        bottomDispAnimator.Play("Disp Bottom Fade In");
        // if(!isBroken)
        //     OpenDoors();
        // else
        //     CloseDoors();
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
       // isAnimating = true;
        
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

       // isAnimating = false;
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

    public void Break(bool fromSave = false)
    {
        isBroken = true;
        if(!fromSave)
        {
            StartCoroutine(BreakAnimation());
        }
        else
        {
            SetBroken();
        }
    }

    private IEnumerator BreakAnimation()
    {
        yield return new WaitForSeconds(3);
        AudioManager.Play("Fall");
        CameraShake.ShakeIncrease(0.75f, 1f);
        yield return new WaitForSeconds(0.75f);
        for(int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitCircle * 1.5f;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, bottompos.transform.position + random);
        }
        AudioManager.Play("Slide Explosion");
        CameraShake.ShakeConstant(0.5f, 2f);
        SetBroken();
    }

    private void SetBroken()
    {
        CloseDoors();
        foreach(GameObject go in brokenObj)
        {
            go.SetActive(true);
        }
        minecartElevator.isInBreakingAnimation = false;
    }

    public void Repair(bool fromSave = false)
    {
        isBroken = false;
        OpenDoors();
        //delete extra sprites or whatnot
        foreach(GameObject go in brokenObj)
        {
            go.SetActive(false);
        }
        // foreach(GameObject go in deactivateOnFix)
        // {
        //     go.SetActive(false);
        //     {
        //         if(!fromSave) {
        //             for(int i = 0; i < 10; i++)
        //             {
        //                 Vector3 random = Random.insideUnitCircle * 1.5f;
        //                 ParticleManager.SpawnParticle(ParticleType.SmokePoof, go.transform.position + random);
        //             }
        //         }
        //     }
        // }
    }
}
