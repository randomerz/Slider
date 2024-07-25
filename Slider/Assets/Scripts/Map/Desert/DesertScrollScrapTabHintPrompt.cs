using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertScrollScrapTabHintPrompt : MonoBehaviour, ISavable
{
    private bool promptedPreviously = false;

    [SerializeField] private PlayerActionHints playerActionHints;

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
        if (!promptedPreviously && PlayerInventory.Contains("Scroll Scrap"))
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
