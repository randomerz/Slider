using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveChadGameCutscene : MonoBehaviour
{
    private const string CUTSCENE_STARTED_SAVE_STRING = "CavesRobotsVsAliensStarted";
    private const string CUTSCENE_FINISHED_SAVE_STRING = "CavesRobotsVsAliensFinished";

    private bool startedCutscene;

    public ParticleSystem smokeParticles;
    public GameObject computerGameObject;
    public Animator computerAnimator;
    public SpriteRenderer computerTable;
    public Sprite explodedComputerTableSprite;

    // Start is called before the first frame update
    void Start()
    {
        if (SaveSystem.Current.GetBool(CUTSCENE_FINISHED_SAVE_STRING))
        {
            FinishCutscene();
        }
    }

    public void StartCutscene()
    {
        if (!startedCutscene)
        {
            StartCoroutine(_StartCutscene());
        }
    }

    private IEnumerator _StartCutscene()
    {
        SaveSystem.Current.SetBool(CUTSCENE_STARTED_SAVE_STRING, true);
        startedCutscene = true;
        
        // Start animation
        computerAnimator.Play("CaveComputerCutscene");

        yield return new WaitForSeconds(2);

        // Start smoking
        smokeParticles.Play();

        yield return new WaitForSeconds(1.5f);
    }

    public void ExplodeComputer()
    {
        // Explode
        smokeParticles.Stop();
        AudioManager.Play("Slide Explosion");
        CameraShake.Shake(1, 0.75f);

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, computerGameObject.transform.position, computerGameObject.transform.parent);

        SaveSystem.Current.SetBool(CUTSCENE_FINISHED_SAVE_STRING, true);

        FinishCutscene();
    }

    private void FinishCutscene()
    {
        computerGameObject.SetActive(false);
        computerTable.sprite = explodedComputerTableSprite;
    }
}
