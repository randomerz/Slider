using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPickupEffect : MonoBehaviour
{
    public AnimationCurve soundDampenCurve;
    public float soundDampenLength = 2;

    public GameObject maskObject;
    public Animator animator;
    public TextMeshProUGUI itemText;
    public Image itemImage;
    // public SpriteRenderer playerSprite;

    public static ItemPickupEffect _instance;

    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {

    }

    public static void StartCutscene(Sprite itemSprite, string itemName, System.Action onTextVisibleCallback=null)
    {
        _instance.itemText.text = itemName + " Acquired!";
        _instance.itemImage.sprite = itemSprite;
        _instance.StartCoroutine(_instance.Cutscene(onTextVisibleCallback));
        _instance.StartCoroutine(_instance.DampenMusic());
    }

    private IEnumerator Cutscene(System.Action onTextVisibleCallback=null)
    {
        NPC.gDialogueEnabled = false;
        maskObject.SetActive(true);
        animator.SetBool("isVisible", true);
        AudioManager.Play("Item Pick Up");

        UIManager.canOpenMenus = false;
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
        UIManager.canOpenMenus = true;
        Player.SetCanMove(true);
        Player.GetSpriteRenderer().sortingLayerName = "Entity";
        NPC.gDialogueEnabled = true;
    }

    private IEnumerator DampenMusic()
    {
        float t = 0;

        float origVolume = AudioManager.GetMusicVolume();

        while (t < soundDampenLength)
        {
            AudioManager.SetMusicVolume(origVolume * soundDampenCurve.Evaluate(t / soundDampenLength));

            yield return null;
            t += Time.deltaTime;
        }

        AudioManager.SetMusicVolume(origVolume);
    }
}
