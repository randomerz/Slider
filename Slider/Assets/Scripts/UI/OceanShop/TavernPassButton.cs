using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TavernPassButton : MonoBehaviour 
{
    public bool isComplete;

    public string rewardName;
    public Image rewardImage;
    public Image image;

    public Sprite defaultSprite;
    public Sprite completedSprite;
    public Sprite selectedSprite;
    
    public GameObject RTImage;
    public new ParticleSystem particleSystem;
    private float EFFECT_DURATION = 5f;

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

    public void PlayEffect()
    {
        StartCoroutine(EffectCoroutine());
    }

    private IEnumerator EffectCoroutine()
    {
        RTImage.SetActive(true);
        particleSystem.Play();
        AudioManager.Play("Hat Click");
        yield return new WaitForSeconds(EFFECT_DURATION);
        RTImage.SetActive(false);
    }
}