using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TavernPassRewardEffect : MonoBehaviour
{
    public float animationDuration;
    public AnimationCurve animationCurve;
    public float stayDuration;
    
    public RectTransform maskRectTransform;
    public TextMeshProUGUI displayText;
    public Image collectibleImage;

    private const float MAX_MASK_SIZE = 480;
    private const float SOUND_DAMPEN_LENGTH = 2;

    public void StartEffect(string name, Sprite sprite, System.Action onTextVisibleCallback=null, System.Action onEndEffectCallback=null)
    {
        InitEffect(name, sprite);
        StartCoroutine(Effect(onTextVisibleCallback, onEndEffectCallback));
    }

    public IEnumerator StartEffectCoroutine(string name, Sprite sprite, System.Action onTextVisibleCallback=null, System.Action onEndEffectCallback=null)
    {
        InitEffect(name, sprite);
        yield return Effect(onTextVisibleCallback, onEndEffectCallback);
    }

    private void InitEffect(string name, Sprite sprite)
    {
        gameObject.SetActive(true);
        maskRectTransform.sizeDelta = Vector2.zero;

        displayText.text = $"{name} Acquired!";
        collectibleImage.sprite = sprite;

        AudioManager.DampenMusic(this, 0.2f, SOUND_DAMPEN_LENGTH);
    }

    private IEnumerator Effect(System.Action onTextVisibleCallback=null, System.Action onEndEffectCallback=null)
    {
        AudioManager.Play("Ocean Pick Up");
        displayText.gameObject.SetActive(false);

        float t = 0;
        while (t < animationDuration)
        {
            float width = MAX_MASK_SIZE * animationCurve.Evaluate(t / animationDuration);
            maskRectTransform.sizeDelta = width * Vector2.one;

            yield return null;
            t += Time.deltaTime;
        }

        yield return new WaitForSeconds(0.25f);

        displayText.gameObject.SetActive(true);
        onTextVisibleCallback?.Invoke();

        yield return new WaitForSeconds(stayDuration - 0.25f);

        t = 0;
        while (t < animationDuration)
        {
            float width = MAX_MASK_SIZE * animationCurve.Evaluate(1 - (t / animationDuration));
            maskRectTransform.sizeDelta = width * Vector2.one;

            yield return null;
            t += Time.deltaTime;
        }
        
        onEndEffectCallback?.Invoke();
        gameObject.SetActive(false);
    }
}
