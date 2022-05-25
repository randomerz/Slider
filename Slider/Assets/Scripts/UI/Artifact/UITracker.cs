using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITracker : MonoBehaviour
{


    public GameObject target;
    public Image image;
    private float minY = -75; // for if the object being tracked is in the house or not

    private bool shouldBlink = false;
    private Sprite defaultSprite;
    private Sprite blinkSprite;
    private float defaultBlinkTime;
    private float blinkTime;
    private float timeToSwap;

    private void Update() 
    {
        if (shouldBlink)
        {
            if (blinkTime > 0 && timeToSwap < 0.5f)//blinkTime)
            {
                if (timeToSwap < 0)
                {
                    image.sprite = defaultSprite;
                    timeToSwap = 1;//2 * blinkTime;
                }
                else
                {
                    // set to blink sprite
                    image.sprite = blinkSprite;
                }
            }
            else
            {
                image.sprite = defaultSprite;
            }

            timeToSwap -= Time.deltaTime;
            blinkTime -= Time.deltaTime;
        }
    }

    private void OnEnable() 
    {
        if (shouldBlink)
        {
            blinkTime = defaultBlinkTime;
            timeToSwap = 1;
        }
    }

    public void SetSprite(Sprite sprite)
    {
        defaultSprite = sprite;
        image.sprite = sprite;

        // RectTransform rt = GetComponent<RectTransform>();
        image.SetNativeSize();
    }

    public STile GetSTile() {
        return SGrid.current.GetStileUnderneath(target);
    }

    public Vector2 GetPosition() {
        return target.transform.position;
    }

    public bool GetIsInHouse() {
        if(target.transform.position.y < minY)
            return true;
        return false;
    }

    public void StartBlinking(Sprite blinkSprite, float blinkTime)
    {
        shouldBlink = true;
        defaultSprite = image.sprite;
        this.blinkSprite = blinkSprite;
        this.defaultBlinkTime = blinkTime;
        this.blinkTime = blinkTime;
        timeToSwap = 1;//2 * blinkTime;
    }
}
