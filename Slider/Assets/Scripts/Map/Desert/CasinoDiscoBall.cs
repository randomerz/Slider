using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasinoDiscoBall : MonoBehaviour, ISavable
{
    public Animator ballAnimator;
    public Animator floorAnimator;
    public Collider2D ballCollider;
    private bool didFall;

    [Header("External References")]
    public DiceGizmo dice1;
    public DiceGizmo dice2;

    private const string SAVE_STRING = "desertDiscoBallFell";
    private const string CHAD_FLAG1 = "desertDiscoBallContactGround";
    private const string CHAD_FLAG2 = "desertSlideAfterBallFell";
    private const string FALL_ANIMTION = "Fall";
    private const string BROKEN_IDLE_ANIMTION = "BrokenIdle";

    private void OnEnable() 
    {
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
    }

    private void OnDisable() 
    {
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
    }
    
    public void Load(SaveProfile profile)
    {
        didFall = profile.GetBool(SAVE_STRING);

        if (didFall)
        {
            ballAnimator.Play(BROKEN_IDLE_ANIMTION);
            floorAnimator.Play(BROKEN_IDLE_ANIMTION);
            EnableCollider();
        }
    }

    public void Save()
    {
        SaveSystem.Current.SetBool(SAVE_STRING, didFall);
    }

    public void Fall()
    {
        if (didFall)
            return;
        didFall = true;
        SaveSystem.Current.SetBool(SAVE_STRING, didFall);

        AudioManager.Play("Glass Clink");

        ballAnimator.Play(FALL_ANIMTION);
        floorAnimator.Play(FALL_ANIMTION);
    }

    public void EnableCollider()
    {
        ballCollider.enabled = true;
    }

    public void DoScreenShake()
    {
        AudioManager.Play("Slide Explosion");
        CameraShake.Shake(1f, 0.5f); // values from Anchor

        // for dice
        SaveSystem.Current.SetBool(CHAD_FLAG1, true);
        dice2.changeValue(1);
    }



    public void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if ((e.stile.islandId == 5 || e.stile.islandId == 6) && didFall)
        {
            SaveSystem.Current.SetBool(CHAD_FLAG2, true);
        }
    }

    public void DoDiceCutscene()
    {
        StartCoroutine(IDoDiceCutscene());
    }

    private IEnumerator IDoDiceCutscene()
    {
        dice1.changeValue(5);
        dice2.changeValue(4);

        yield return new WaitForSeconds(3.75f);

        Fall();
    }
}
