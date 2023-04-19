using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertArcheologistWalking : MonoBehaviour
{
    public GameObject diggingParticles;
    public NPC npc;
    public int conditionWithWalks; // this is really sketch

    [SerializeField] private float diggingDuration;

    private NPCConditionals npcCondition;
    private int currentWalkIndex;
    private bool doneDigging;
    private bool finishWandering;
    private bool finishedWandering;

    private const string DESERT_ARCH_WANDERING = "desertArchTailWandering";
    
    private void Start() {
        
        npcCondition = npc.Conds[conditionWithWalks];
        if (npcCondition.dialogueChain[0].dialogue != "one sec... it should be here... guarded by the beast...")
        {
            Debug.LogWarning("hey bestie you changed the dialogue you might wanna check this");
        }

        if (SaveSystem.Current.GetBool(DESERT_ARCH_WANDERING))
        {
            StartWandering();
        }
    }

    public void StartWandering()
    {
        StartCoroutine(Wander());
    }

    private IEnumerator Wander()
    {
        while (!finishedWandering)
        {
            if (finishWandering && currentWalkIndex == 0)
            {
                npc.AdvanceDialogueChain();
                finishedWandering = true;
                yield break;
            }

            npc.StartWalkAtIndex(currentWalkIndex);
            currentWalkIndex = (currentWalkIndex + 1) % npcCondition.walks.Count;
            doneDigging = false;

            yield return new WaitUntil(() => doneDigging);
        }
    }

    public void FinishWandering()
    {
        finishWandering = true;
    }

    public void StartDig()
    {
        StartCoroutine(Dig());
    }

    private IEnumerator Dig(System.Action callback=null)
    {
        Instantiate(diggingParticles, transform.position, Quaternion.identity, transform.parent);
        StartCoroutine(DigSounds());

        yield return new WaitForSeconds(diggingDuration);

        doneDigging = true;

        callback?.Invoke();
    }

    private IEnumerator DigSounds()
    {
        for (int i = 0; i < 4; i++)
        {
            AudioManager.PlayWithVolume("Hat Click", 0.5f);

            yield return new WaitForSeconds(0.75f);
        }
    }
}
