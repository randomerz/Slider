using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPickupEffect : MonoBehaviour
{
    public GameObject maskObject;
    public Animator animator;
    public TextMeshProUGUI itemText;
    public Image itemImage;
    public SpriteRenderer playerSprite;

    public static ItemPickupEffect _instance;

    void Awake()
    {
        _instance = this;
    }

    private void Start()
    {

    }

    public static void StartCutscene(Sprite itemSprite, string itemName)
    {
        _instance.itemText.text = itemName + " Acquired!";
        _instance.itemImage.sprite = itemSprite;
        _instance.StartCoroutine(_instance.Cutscene());
    }

    private IEnumerator Cutscene()
    {
        maskObject.SetActive(true);
        animator.SetBool("isVisible", true);
        // play sound
        UIManager.canOpenMenus = false;
        Player.canMove = false;

        playerSprite.sortingLayerName = "UI_Particle";

        yield return new WaitForSeconds(0.75f);

        itemText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.25f);

        animator.SetBool("isVisible", false);
        itemText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        maskObject.SetActive(false);
        UIManager.canOpenMenus = true;
        Player.canMove = true;
        playerSprite.sortingLayerName = "Entity";
    }
}
