using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Localization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPickupEffect : MonoBehaviour, IDialogueTableProvider
{
    public static System.EventHandler<System.EventArgs> OnCutsceneStart;

    public AnimationCurve soundDampenCurve;
    public float soundDampenLength = 2;

    public GameObject maskObject;
    public Animator animator;
    public TextMeshProUGUI itemText;
    public Image itemImage;
    // public SpriteRenderer playerSprite;

    public static ItemPickupEffect _instance;

    enum ItemPickupEffectStrings
    {
        Acquired
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<ItemPickupEffectStrings, string>()
        {
            { ItemPickupEffectStrings.Acquired, "<item/> Acquired!"}
        });

    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {

    }

    public static void StartCutscene(Sprite itemSprite, string itemName, System.Action onTextVisibleCallback=null)
    {
        _instance.itemText.text = (_instance as IDialogueTableProvider).Interpolate(
            _instance.GetLocalizedSingle(ItemPickupEffectStrings.Acquired),
            new()
            {
                {"item", LocalizationLoader.LoadCollectibleTranslation(itemName, SGrid.Current.GetArea())}
            });
        _instance.itemImage.sprite = itemSprite;
        _instance.itemImage.SetNativeSize();
        _instance.itemImage.rectTransform.localScale = 1f / 16f * Vector3.one;
        _instance.StartCoroutine(_instance.Cutscene(onTextVisibleCallback));
        AudioManager.DampenMusic(itemSprite, 0.2f, _instance.soundDampenLength);

        OnCutsceneStart?.Invoke(_instance, null);
    }

    private IEnumerator Cutscene(System.Action onTextVisibleCallback=null)
    {
        NPCDialogueContext.dialogueEnabledAllNPC = false;
        maskObject.SetActive(true);
        animator.SetBool("isVisible", true);

        string pickupArea = "Village";
        if (SGrid.Current != null)
        {
            pickupArea = SGrid.Current.GetArea().ToString();
        }
        AudioManager.PickSound($"{pickupArea} Pick Up").WithPriorityOverDucking(true).WithSingleInstanceKey("Pick Up").AndPlay();

        PauseManager.AddPauseRestriction(owner: gameObject);
        Player.SetCanMove(false);

        Player.GetSpriteRenderer().sortingLayerName = "ScreenEffects";

        yield return new WaitForSeconds(0.75f);

        itemText.gameObject.SetActive(true);

        if (onTextVisibleCallback != null) {
            onTextVisibleCallback();
        }

        yield return new WaitForSeconds(1.25f);

        animator.SetBool("isVisible", false);
        itemText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        maskObject.SetActive(false);
        PauseManager.RemovePauseRestriction(owner: gameObject);
        Player.SetCanMove(true);
        Player.GetSpriteRenderer().sortingLayerName = "Entity";
        NPCDialogueContext.dialogueEnabledAllNPC = true;
    }
}
