using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertScrollScrapTabHintPrompt : MonoBehaviour, ISavable
{
    private bool promptedPreviously = false;
    private bool sceneMostlyInitialized = false;

    [SerializeField] private PlayerActionHints playerActionHints;

    private void Start()
    {
        sceneMostlyInitialized = true;
        CheckHint();
    }

    public void Load(SaveProfile profile)
    {
        promptedPreviously = SaveSystem.Current.GetBool("DesertPromptedScrollScrapHint");
    }

    public void Save()
    {
        SaveSystem.Current.SetBool("DesertPromptedScrollScrapHint", promptedPreviously);
    }

    private void OnEnable()
    {
        CheckHint();
    }

    private void CheckHint()
    {
        if (!promptedPreviously && sceneMostlyInitialized && PlayerInventory.Contains("Scroll Scrap"))
        {
            // playerActionHints.TriggerHint("scrollscrap");
            UIEffects.StartSpotlight(
                new Vector2(95, 50), 
                48, 
                1.5f,
                () => { UIArtifactMenus.SetKeepArtifactOpen(true); },
                () => { UIArtifactMenus.SetKeepArtifactOpen(false); }
            );
            promptedPreviously = true;
        }
    }
}
