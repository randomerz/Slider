using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAbductionController : MonoBehaviour
{
    private bool isIntroCutsceneFinished;
    private float timeSinceIntroCutscene;
    private bool isPicked;
    private float timeSincePicked;
    private bool hasStartedAbducting;

    [SerializeField] private Item catItem;
    [SerializeField] private Animator catAnimator;
    [SerializeField] private SinWaveAnimation catSinWave;
    [SerializeField] private CatBubbleController bubble;

    [SerializeField] private Transform catBeachLandingSpot;
    [SerializeField] private GameObject alienLogSign;

    public AnimationCurve lerpCurve;

    private const float TIME_ABDUCT_NORMAL = 6; // seconds after intro cutscene finishes
    private const float TIME_ABDUCT_PICKED = 3; // seconds after finish pick
    private const float TIME_ABDUCT_FINISH_LERP = 2; // seconds while lerping between og and sine wave

    private const string EXPLORESSE_SEQ_SAVE_STRING = "villageExploresseSequenceDone";

    
    void Start()
    {
        // if HAS_ENTERED_BOAT_STRING => remove from scene or make float away again
        if (SaveSystem.Current.GetBool(VillageGrid.INTRO_CUTSCENE_SAVE_STRING))
        {
            SaveSystem.Current.SetBool(EXPLORESSE_SEQ_SAVE_STRING, true);

            catItem.transform.position = catBeachLandingSpot.position;

            catItem.SetCollider(false);
            catItem.SetSortingOrder(99);
            catItem.transform.SetParent(null);
        }

        if (PlayerInventory.Contains("Boating License"))
        {
            alienLogSign.SetActive(true);
            catItem.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!hasStartedAbducting)
        {
            if (isIntroCutsceneFinished)
            {
                timeSinceIntroCutscene += Time.deltaTime;

                if (timeSinceIntroCutscene > TIME_ABDUCT_NORMAL)
                {
                    StartAbducting();
                }
            }
            if (isPicked)
            {
                timeSincePicked += Time.deltaTime;

                if (timeSincePicked > TIME_ABDUCT_PICKED)
                {
                    StartAbducting();
                }
            }
        }
    }

    public void SetIntroCutsceneFinished()
    {
        isIntroCutsceneFinished = true;
    }

    public void SetPickedUp()
    {
        isPicked = true;

        timeSinceIntroCutscene = 0; // reset this so the player can enjoy carrying cat
    }

    private void StartAbducting()
    {
        if (hasStartedAbducting)
            return;
        hasStartedAbducting = true;

        PlayerInventory.RemoveItem();
        catItem.SetCollider(false);
        catItem.SetSortingOrder(99);
        catItem.transform.SetParent(null);

        StartCoroutine(AbductCat());
    }

    private IEnumerator AbductCat()
    {
        CameraShake.Shake(0.5f, 0.5f);
        AudioManager.PickSound("Slide Rumble").WithVolume(0.5f).WithAttachmentToTransform(catItem.transform).AndPlay();
        AudioManager.PickSound("MagicChimes1").WithVolume(0.5f).AndPlay();

        bubble.SetBubbleActive(true);
        catAnimator.SetBool("isAwake", true);

        yield return new WaitForSeconds(1);

        AudioManager.Play("Meow");

        StartCoroutine(DoExploresseSequence());

        catSinWave.enabled = true;
        catSinWave.horizontalVelocity = 2f;

        Vector3 startPos = catItem.transform.position;
    
        float t = 0;
        while (t < TIME_ABDUCT_FINISH_LERP)
        {
            float x = lerpCurve.Evaluate(t / TIME_ABDUCT_FINISH_LERP);

            catItem.transform.position = Vector3.Lerp(startPos, catSinWave.transform.position, x);

            t += Time.deltaTime;
            yield return null;
        }

        bool playerOnBeach = false;

        // once cat goes over water, let him chill
        yield return new WaitUntil(() => {
            playerOnBeach = playerOnBeach || Player.GetPosition().x > 43;
            if (Player.GetPosition().x > catSinWave.transform.position.x)
            {
                catSinWave.horizontalVelocity = 2.5f;
            }
            catItem.transform.position = catSinWave.transform.position;
            return catSinWave.transform.position.x > catBeachLandingSpot.position.x - 1;
        });

        // pop!
        catSinWave.enabled = false;
        catSinWave.horizontalVelocity = 0;
        bubble.SetBubbleActive(false);
        AudioManager.Play("Pop");
        AudioManager.PlayWithVolume("Fall", 0.5f);

        t = 0;
        startPos = catItem.transform.position;
        while (t < 1)
        {
            float x = lerpCurve.Evaluate(t);

            catItem.transform.position = Vector3.Lerp(startPos, catBeachLandingSpot.transform.position, x);

            t += Time.deltaTime;
            yield return null;
        }

        catItem.transform.position = catBeachLandingSpot.transform.position;

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, catItem.transform.position, catItem.transform);
        AudioManager.Play("Hat Click");

        yield return new WaitForSeconds(1);

        catAnimator.SetBool("isAwake", false);
    }

    private IEnumerator DoExploresseSequence()
    {
        yield return new WaitForSeconds(1.4f);

        SaveSystem.Current.SetBool("villageCatFloatAway", true);
        // TODO: play door sound
        AudioManager.Play("Hat Click");
    }
}
