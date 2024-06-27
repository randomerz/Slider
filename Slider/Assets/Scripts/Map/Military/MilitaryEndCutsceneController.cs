using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryEndCutsceneController : MonoBehaviour
{
    public NPC alienCommander;
    public GameObject alienSkull;
    public GameObject fightParticlesPrefab;

    public List<NPC> alienNPCs = new();
    public List<FlashWhiteSprite> flashWhites;

    private Coroutine coroutine;

    private const string COMMANDER_TALKED_STRING = "militaryAlienCommanderTalked";
    private const string START_FIGHT_STRING = "militaryAlienFightStart";
    private const string END_FIGHT_STRING = "militaryAliensAttacked";
    private const string MILITARY_COMPLETE_STRING = "militaryComplete";

    private void Start()
    {
        if (SaveSystem.Current.GetBool(END_FIGHT_STRING))
        {
            FinishFight();
        }

        if (alienNPCs.Count != 3)
        {
            Debug.LogError($"Warning: make sure to update cutscene");
        }
    }

    private void OnDestroy()
    {
        if (coroutine != null)
        {
            SaveSystem.Current.SetBool(END_FIGHT_STRING, true);
            SaveSystem.Current.SetBool(MILITARY_COMPLETE_STRING, true);
        }
    }

    public void AddTrackerToCommander()
    {
        UITrackerManager.AddNewTracker(
            alienCommander.gameObject, 
            UITrackerManager.DefaultSprites.exclamation, 
            UITrackerManager.DefaultSprites.none,
            blinkTime: 3
        );

        CoroutineUtils.ExecuteAfterDelay(
            () => {
                UIEffects.FlashWhite();
                CameraShake.Shake(0.5f, 0.25f);
                AudioManager.PlayWithVolume("Slide Explosion", 0.1f);
                AudioManager.Play("Portal");
            },
            this,
            1
        );
    }

    public void QueueAlienComplaints()
    {
        if (coroutine != null)
        {
            return;
        }

        SaveSystem.Current.SetBool(COMMANDER_TALKED_STRING, true);
        coroutine = StartCoroutine(DoQueueAlienComplaints());
    }

    private IEnumerator DoQueueAlienComplaints()
    {
        yield return new WaitForSeconds(0.5f);

        alienNPCs[0].TypeCurrentDialogue();

        yield return new WaitForSeconds(0.25f);

        alienNPCs[1].TypeCurrentDialogue();

        yield return new WaitForSeconds(0.75f);

        alienNPCs[2].TypeCurrentDialogue();

        yield return new WaitForSeconds(3f);

        StartAlienFight();
    }

    public void StartAlienFight()
    {
        SaveSystem.Current.SetBool(START_FIGHT_STRING, true);
        MilitaryTurnAnimator.CurrentGlobalAnimationsSpeed = MilitaryTurnAnimator.Speed.Slow;
        
        GameObject go = Instantiate(fightParticlesPrefab, alienCommander.transform.position, Quaternion.identity);

        coroutine = CoroutineUtils.ExecuteAfterDelay(
            () => {
                SaveSystem.Current.SetBool(END_FIGHT_STRING, true);
                SaveSystem.Current.SetBool(MILITARY_COMPLETE_STRING, true);
                AudioManager.Play("Hurt");
                coroutine = null;
                FinishFight();
            },
            this,
            MGFight.FightDuration
        );

        CoroutineUtils.ExecuteAfterDelay(
            () => FlashForDuration(MGFight.FightDuration - 0.5f),
            this,
            0.5f
        );
        

        // From MGFight.cs
        AudioManager.PickSound("UI Click").WithPitch(0.6f).AndPlay();
        for (float i = 0.5f; i < MGFight.FightDuration; i += 0.5f)
        {
            CoroutineUtils.ExecuteAfterDelay(
                () => AudioManager.PickSound("UI Click").WithPitch(0.5f).AndPlay(),
                this,
                i
            );
        }
    }

    public void FlashForDuration(float seconds)
    {
        FlashWhite((int)(seconds / (flashWhites[0].flashTime * 2)));
    }

    private void FlashWhite(int num, System.Action callback=null)
    {
        foreach (FlashWhiteSprite f in flashWhites)
        {
            f.Flash(num, callback);
        }
    }

    private void FinishFight()
    {
        alienCommander.gameObject.SetActive(false);
        alienSkull.SetActive(true);
    }
}