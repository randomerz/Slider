using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionPool : MonoBehaviour
{
    public List<Animator> obeliskAnimators;
    public List<GameObject> toggleOn;
    public ConditionChecker conditionChecker;
    public GemManager gemManager;
    public GameObject artifact;
    public GameObject gem;
    public GameObject npc;
    public List<GameObject> cutsceneParticles;

    private const string CUTSCENE_SAVE_STRING = "MagiTechReflectionTurnInCutscene";

    private void Start()
    {
        if (SaveSystem.Current.GetBool(CUTSCENE_SAVE_STRING))
        {
            TurnInHelper();
        }
        else
        {
            gem.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SGrid.OnSTileEnabled += Check; 
        ArtifactTabManager.AfterScrollRearrage += Check;

    }

    private void OnDisable()
    {
        SGrid.OnSTileEnabled -= Check;
        ArtifactTabManager.AfterScrollRearrage -= Check;
    }

    #region  check
    private void Check(object sender, EventArgs e)
    {
        Check();
    }

    private void Check(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        Check();
    }

    private void Check(object sender, SGrid.OnGridMoveArgs e)
    {
        Check();
    }

    private void Check(object sender, SGrid.OnSTileEnabledArgs e)
    {
        Check();
    }


    private void Check()
    {
        conditionChecker.CheckConditions();
    }
#endregion

    public void TurnOnReflectionPool()
    {
        foreach(Animator a in obeliskAnimators)
        {
            a.Play("MagiTechObeliskOn");
        }
        foreach(GameObject g in toggleOn)
        {
            g.SetActive(true);
        }
    }

    public void TurnOffReflectionPool()
    {
        foreach(Animator a in obeliskAnimators)
        {
            a.Play("MagiTechObeliskOff");
        }
        foreach(GameObject g in toggleOn)
        {
            g.SetActive(false);
        }
    }

    public void TurnInArtifact()
    {
        StartCoroutine(TurnInArtifactCutscene());
    }

    private IEnumerator TurnInArtifactCutscene()
    {
        SaveSystem.Current.SetBool(CUTSCENE_SAVE_STRING, true);
        ChadChirp.OnTryChirp?.Invoke(this, new ChadChirp.ChadChirpArgs { id = "ReflectionPoolCutscene" });

        foreach(GameObject go in cutsceneParticles)
            go.SetActive(true);
        AudioManager.Play("Slide Rumble");
        //UIManager.CloseUI();
        PauseManager.SetPauseState(false);
        UIArtifactMenus.TurnInArtifact();
        npc.SetActive(false);
        CameraShake.ShakeIncrease(4, 0.5f);
        yield return new WaitForSeconds(3.5f);
        UIEffects.FlashWhite(
            () => {
                AudioManager.Play("Puzzle Complete");
                TurnInHelper(); 
            }, 
            () => {
                EndTurnIn();
            }
        );
    }

    private void TurnInHelper()
    {
        SaveSystem.Current.SetBool("magitechTurnedInArtifact", true);
        UIArtifactWorldMap.SetAreaStatus(Area.MagiTech, ArtifactWorldMapArea.AreaStatus.color);
        AchievementManager.SetAchievementStat("completedMagitech", false, 1);
        artifact.SetActive(true);
        gem.SetActive(!gemManager.HasAreaGem(Area.MagiTech));
    }

    private void EndTurnIn()
    {
        npc.SetActive(true);
    }
}
