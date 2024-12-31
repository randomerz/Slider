using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;
using UnityEngine.UI;

public class TavernPassButton : MonoBehaviour
{
    public bool isComplete;

    // Set by TavernPassManager
    public string rewardName;
    // Set in inspector
    public int rewardIndex;
    public Image rewardImage;
    public Image image;

    public Sprite defaultSprite;
    public Sprite completedSprite;
    public Sprite selectedSprite;
    
    public GameObject RTImage;
    public new ParticleSystem particleSystem;
    public FlashWhiteImage flashWhite;
    private float EFFECT_DURATION = 5f;

    private void OnEnable()
    {
        RTImage.SetActive(false);
    }

    public void Deselect()
    {
        image.sprite = isComplete ? completedSprite : defaultSprite;
    }

    public void Select()
    {
        image.sprite = selectedSprite;
    }

    public void SetComplete(bool value)
    {
        isComplete = value;

        if (image.sprite != selectedSprite)
            image.sprite = completedSprite;
    }

    public void DisableRenderTexture()
    {
        RTImage.SetActive(false);
    }

    public void PlayEffect()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(EffectCoroutine());
        }
    }

    private IEnumerator EffectCoroutine()
    {
        RTImage.SetActive(true);
        particleSystem.Play();
        flashWhite.Flash(1);
        AudioManager.Play("Hat Click");
        yield return new WaitForSeconds(EFFECT_DURATION);
        RTImage.SetActive(false);
    }
}