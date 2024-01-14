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
    private bool shouldCheckOffMap = false;
    private Sprite defaultSprite;
    private Sprite blinkSprite;
    private Sprite defaultOffMapSprite;
    private Sprite blinkOffMapSprite;
    private float defaultBlinkTime;
    private float blinkTime;
    private float timeToSwap;
    private float defaultTimeUntilBlinkRepeat = -1;
    private float timeUntilBlinkRepeat; // start countdown same time as blink starts

    private void Update() 
    {
        if (shouldBlink)
        {
            if (blinkTime > 0)
            {
                if (timeToSwap > 0.5f || 0 > timeToSwap)
                {
                    image.sprite = DetermineCurrentSprite(false);
                }
                else
                {
                    // set to blink sprite
                    image.sprite = DetermineCurrentSprite(true);
                }
            }
            else
            {
                if (timeUntilBlinkRepeat < 0 && defaultTimeUntilBlinkRepeat != -1)
                {
                    // repeat blink
                    blinkTime = defaultBlinkTime;
                    timeToSwap = 1;
                    timeUntilBlinkRepeat = defaultTimeUntilBlinkRepeat;
                }
                image.sprite = DetermineCurrentSprite(false);
                image.SetNativeSize();
            }

            timeToSwap -= Time.deltaTime;
            blinkTime -= Time.deltaTime;
            timeUntilBlinkRepeat -= Time.deltaTime;

            if (timeToSwap < 0)
            {
                timeToSwap = 1;
            }

            image.SetNativeSize();
        }
    }

    private Sprite DetermineCurrentSprite(bool blink)
    {
        if (shouldCheckOffMap && (GetSTile(out _) == null))
        {
            // offmap sprite
            return blink ? blinkOffMapSprite : defaultOffMapSprite;
        }
        else
        {
            return blink ? blinkSprite : defaultSprite;
        }
    }

    private bool OnMirageTile()
    {
        if(MirageSTileManager.GetInstance() == null) return false;
        else return MirageSTileManager.GetInstance().IsObjectOnMirage(target.transform, out _);
    }

    private void OnEnable() 
    {
        if (shouldBlink)
        {
            blinkTime = defaultBlinkTime;
            timeToSwap = 1;
            timeUntilBlinkRepeat = defaultTimeUntilBlinkRepeat;
        }
    }

    public void SetSprite(Sprite sprite)
    {
        defaultSprite = sprite;
        image.sprite = sprite;

        // RectTransform rt = GetComponent<RectTransform>();
        image.SetNativeSize();
    }

    public GameObject GetSTile(out int islandId) {
        islandId = -1;
        if(MirageSTileManager.GetInstance() != null)
        {
            var m = MirageSTileManager.GetInstance();
            if(m.IsObjectOnMirage(target.transform, out islandId))
            {
                var t = m.GetMirageTileForUI(islandId);
                islandId = m.GetButtonIslandID(islandId);
                return t;
            }
        }
        STile tile = SGrid.GetSTileUnderneath(target);
        if(tile != null)
        {
            islandId = tile.islandId;
            return tile.gameObject;
        }
        return null; 
    }

    public Vector2 GetPosition() {
        return target.transform.position;
    }

    public bool GetIsInHouse() {
        if(target.transform.position.y < minY)
            return true;
        return false;
    }

    public void StartBlinking(Sprite blinkSprite, float blinkTime, float timeUntilBlinkRepeat)
    {
        shouldBlink = true;
        defaultSprite = image.sprite;
        this.blinkSprite = blinkSprite;
        this.defaultBlinkTime = blinkTime;
        this.blinkTime = blinkTime;
        timeToSwap = 1;//2 * blinkTime;
        this.defaultTimeUntilBlinkRepeat = timeUntilBlinkRepeat;
        this.timeUntilBlinkRepeat = timeUntilBlinkRepeat;
    }

    public void SetOffMapSprites(Sprite defaultOffMapSprite, Sprite blinkOffMapSprite)
    {
        shouldCheckOffMap = true;
        this.defaultOffMapSprite = defaultOffMapSprite;
        this.blinkOffMapSprite = blinkOffMapSprite;
    }
}
