using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;

public class AnchorFirstAppearance : MonoBehaviour, IDialogueTableProvider
{
    public Anchor anchor;
    public SpriteRenderer spriteRenderer;
    public PlayerActionHints hints;
    public static event System.EventHandler<System.EventArgs> OnAnchorAcquire;

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<string, string>
        {
            { "Anchor", "Anchor" }
        });

    void Start()
    {
        if (PlayerInventory.Instance.GetHasCollectedAnchor())
        {
            anchor.UnanchorTile();
            Destroy(gameObject);
            return;
        }
        UITrackerManager.AddNewTracker(gameObject, anchor.trackerSprite);
        
        anchor.OnPickUp.AddListener(DoCutscene);
    }

    public void DoCutscene()
    {
        ItemPickupEffect.StartCutscene(spriteRenderer.sprite, this.GetLocalizedSingle("Anchor"));
        anchor.OnPickUp.RemoveListener(DoCutscene);
        OnAnchorAcquire?.Invoke(this, new System.EventArgs{});
        hints.TriggerHint("anchor");

        AchievementManager.SetAchievementStat("collectedAnchor", false, 1);

        Destroy(this);
    }
}
