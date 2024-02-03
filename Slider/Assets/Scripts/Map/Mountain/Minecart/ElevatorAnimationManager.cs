using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator topDispAnimator;
    [SerializeField] private Animator topDoorAnimator;
    [SerializeField] private Animator bottomDispAnimator;
    [SerializeField] private Animator bottomDoorAnimator;

    public List<GameObject> brokenObj;
    public GameObject bottompos;

    public MinecartElevator minecartElevator;
    public List<GameObject> closedDoorColliders;
    public Animator generatorAnimator;

    private void Start() {
        topDispAnimator.Play("Disp Top Fade In");
        bottomDispAnimator.Play("Disp Bottom Fade In");
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
    }

    private void CloseDoors()
    {
        topDoorAnimator.Play("Close");
        bottomDoorAnimator.Play("Close");
        foreach(GameObject go in closedDoorColliders)
        {
            go.SetActive(true);
        }
    }

    private void OpenDoors()
    {
        topDoorAnimator.Play("Open");
        bottomDoorAnimator.Play("Open");
        foreach(GameObject go in closedDoorColliders)
        {
            go.SetActive(false);
        }
    }

    public void Break(bool fromSave = false)
    {
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
        generatorAnimator.Play("Break");
        yield return new WaitForSeconds(1.3f);
        AudioManager.Play("Power Off");
        CameraShake.Shake(0.4f, 0.3f);
        yield return new WaitForSeconds(0.5f);
        AudioManager.Play("Fall");
        CameraShake.ShakeIncrease(1f, 1f);
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitCircle * 1.5f;
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, bottompos.transform.position + random);
        }
        AudioManager.Play("Slide Explosion");
        CameraShake.ShakeConstant(0.5f, 1.5f);
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
        generatorAnimator.Play("Broken");
    }

    public void Repair(bool fromSave = false)
    {
        generatorAnimator.Play("Fixed");
        OpenDoors();
        foreach(GameObject go in brokenObj)
        {
            go.SetActive(false);
        }
        if(!fromSave)
        {
            AudioManager.Play("Power On");
        }
    }
}
