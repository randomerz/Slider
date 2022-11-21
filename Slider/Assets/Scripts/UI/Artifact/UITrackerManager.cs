using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrackerManager : MonoBehaviour
{
    protected static UITrackerManager _instance;
    
    protected virtual Vector2 center => new Vector2(17, 17);
    protected Vector2 position;
    protected Vector2 offset;
    protected ArtifactTileButton currentButton;
    protected STile currentTile;
    protected float scale = 27f/17f;
    protected virtual float centerScale => 37f / 17f;

    public UIArtifactMenus uiArtifactMenus;
    public GameObject artifactPanel;
    public GameObject uiTrackerPrefab;
    public List<UITracker> targets = new List<UITracker>();
    
    // Buffers for when called before initialization
    private struct UITrackerData
    {
        public GameObject target;
        public Sprite sprite;
        public Sprite blinkSprite;
        public Sprite offMapSprite;
        public Sprite offMapBlinkSprite;
        public float blinkTime;

        public UITrackerData(GameObject target, Sprite sprite, Sprite blinkSprite, Sprite offMapSprite, Sprite offMapBlinkSprite, float blinkTime) : this()
        {
            this.target = target;
            this.sprite = sprite;
            this.blinkSprite = blinkSprite;
            this.offMapSprite = offMapSprite;
            this.offMapBlinkSprite = offMapBlinkSprite;
            this.blinkTime = blinkTime;
        }
    }

    private struct UITrackerEnumData
    {
        public GameObject target;
        public DefaultSprites sprite;
        public DefaultSprites blinkSprite;
        public DefaultSprites offMapSprite;
        public DefaultSprites offMapBlinkSprite;
        public float blinkTime;

        public UITrackerEnumData(GameObject target, DefaultSprites sprite, DefaultSprites blinkSprite, DefaultSprites offMapSprite, DefaultSprites offMapBlinkSprite, float blinkTime) : this()
        {
            this.target = target;
            this.sprite = sprite;
            this.blinkSprite = blinkSprite;
            this.offMapSprite = offMapSprite;
            this.offMapBlinkSprite = offMapBlinkSprite;
            this.blinkTime = blinkTime;
        }
    }

    private static List<UITrackerData> uiTrackerBuffer = new List<UITrackerData>();
    private static List<UITrackerEnumData> uiTrackerEnumBuffer = new List<UITrackerEnumData>();
    // private static List<GameObject> objectBuffer = new List<GameObject>();
    // private static List<Sprite> spriteBuffer = new List<Sprite>();
    // private static List<Sprite> blinkSpriteBuffer = new List<Sprite>();
    // private static List<DefaultSprites> enumBuffer = new List<DefaultSprites>();
    // private static List<DefaultSprites> blinkEnumBuffer = new List<DefaultSprites>();
    // private static List<float> blinkTimeBuffer = new List<float>();

    private static List<GameObject> removeBuffer = new List<GameObject>();

    [Header("Default Pins")]
    public Sprite empty;
    public Sprite circle1;
    public Sprite circle2;
    public Sprite circleEmpty;
    public Sprite pin;
    public Sprite exclamation;
    
    public Sprite playerBlackCircle;
    public Sprite playerBlackCircleEmpty;
    public Sprite playerWhiteCircle;
    public Sprite playerWhiteCircleEmpty;


    public enum DefaultSprites {
        none,
        circle1,
        circle2,
        circleEmpty,
        pin,
        exclamation,
        playerBlackCircle,
        playerBlackCircleEmpty,
        playerWhiteCircle,
        playerWhiteCircleEmpty,
    }

    protected virtual void Awake() {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (uiTrackerBuffer.Count > 0) {
            for (int i = 0; i < uiTrackerBuffer.Count; i++) {
                UITrackerData uid = uiTrackerBuffer[i];
                AddNewTracker(uid.target, uid.sprite, uid.blinkSprite, uid.offMapSprite, uid.offMapBlinkSprite, uid.blinkTime);
            }
            uiTrackerBuffer.Clear();
        }

        if (uiTrackerEnumBuffer.Count > 0) {
            for (int i = 0; i < uiTrackerEnumBuffer.Count; i++) {
                UITrackerEnumData uid = uiTrackerEnumBuffer[i];
                AddNewTracker(uid.target, uid.sprite, uid.blinkSprite, uid.offMapSprite, uid.offMapBlinkSprite, uid.blinkTime);
            }
            uiTrackerEnumBuffer.Clear();
        }

        if (removeBuffer.Count > 0) {
            for (int i = removeBuffer.Count; i >= 0; i++) {
                RemoveTracker(removeBuffer[i]);
            }
            removeBuffer.Clear();
        }

        for (int x = 0; x < targets.Count; x++) {
            if (targets[x].target == null) {
                Destroy(targets[x].gameObject);
                targets.RemoveAt(x);
                x--;
                Debug.LogWarning("Removed a tracker pointing to a destroyed object");
            }
        }

        foreach (UITracker t in targets) {
            position = t.GetPosition();
            currentTile = t.GetSTile();
            t.gameObject.SetActive(t.target.activeInHierarchy);

            //S: When target is not on a tile
            if(currentTile == null){
                t.image.rectTransform.SetParent(artifactPanel.GetComponent<RectTransform>());
                CalculateOffsetNullTile();
                //offset = (position - center) * centerScale;
                //offset = new Vector3(Mathf.Clamp(offset.x, -62.5f, 62.5f), Mathf.Clamp(offset.y, -57.5f, 57.5f));

                t.image.rectTransform.anchoredPosition = offset;

                continue;
            }

            CalculateOffset();
            //offset = (position - (Vector2)currentTile.transform.position) * scale;

            currentButton = uiArtifactMenus.uiArtifact.GetButton(currentTile.islandId);
            // Debug.Log("Setting transform of " + t.name);
            t.image.rectTransform.SetParent(currentButton.imageRectTransform);
            
            if(t.GetIsInHouse())
            {
                t.image.rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                t.image.rectTransform.anchoredPosition = offset;
            }
        }
    }

    // C: made into its own method so it can be overriden 
    protected virtual void CalculateOffsetNullTile() 
    {
        offset = (position - center) * centerScale;
        offset = new Vector3(Mathf.Clamp(offset.x, -70f, 70f), Mathf.Clamp(offset.y, -60f, 60f));
    }

    protected virtual void CalculateOffset() 
    {
        offset = (position - (Vector2)currentTile.transform.position) * scale;
    }


    public static void AddNewTracker(
                                    GameObject target, 
                                    DefaultSprites sprite=DefaultSprites.circle1, 
                                    DefaultSprites blinkSprite=DefaultSprites.none, 
                                    DefaultSprites offMapSprite=DefaultSprites.none, 
                                    DefaultSprites offMapBlinkSprite=DefaultSprites.none, 
                                    float blinkTime=-1
                                    ) 
    {
        if (_instance == null) {
            uiTrackerEnumBuffer.Add(new UITrackerEnumData(target, sprite, blinkSprite, offMapSprite, offMapBlinkSprite, blinkTime));
            return;
        }

        AddNewTracker(target, EnumToSprite(sprite), EnumToSprite(blinkSprite), EnumToSprite(offMapSprite), EnumToSprite(offMapBlinkSprite), blinkTime);
    }
    
    public static void AddNewTracker(
                                    GameObject target, 
                                    Sprite sprite, 
                                    Sprite blinkSprite=null, 
                                    Sprite offMapSprite=null, 
                                    Sprite offMapBlinkSprite=null, 
                                    float blinkTime=-1
                                    ) 
    {
        if (_instance == null) {
            uiTrackerBuffer.Add(new UITrackerData(target, sprite, blinkSprite, offMapSprite, offMapBlinkSprite, blinkTime));
            return;
        }

        GameObject tracker = GameObject.Instantiate(_instance.uiTrackerPrefab, _instance.transform);
        UITracker uiTracker = tracker.GetComponent<UITracker>();
        uiTracker.target = target;
        uiTracker.SetSprite(sprite);

        if (blinkTime != -1)
        {
            uiTracker.StartBlinking(blinkSprite, blinkTime);
        }

        if (offMapSprite != null)
        {
            uiTracker.SetOffMapSprites(offMapSprite, offMapBlinkSprite);
        }

        _instance.targets.Add(uiTracker);
    }

    private static Sprite EnumToSprite(DefaultSprites enumValue)
    {
        switch (enumValue)
        {
            case DefaultSprites.none:
                return null;
            case DefaultSprites.circle1:
                return _instance.circle1;
            case DefaultSprites.circle2:
                return _instance.circle2;
            case DefaultSprites.circleEmpty:
                return _instance.circleEmpty;
            case DefaultSprites.pin:
                return _instance.pin;
            case DefaultSprites.exclamation:
                return _instance.exclamation;
            case DefaultSprites.playerBlackCircle:
                return _instance.playerBlackCircle;
            case DefaultSprites.playerBlackCircleEmpty:
                return _instance.playerBlackCircleEmpty;
            case DefaultSprites.playerWhiteCircle:
                return _instance.playerWhiteCircle;
            case DefaultSprites.playerWhiteCircleEmpty:
                return _instance.playerWhiteCircleEmpty;

        }
        Debug.LogWarning("Couldn't find pin!");
        return null;
    }

    // private static void AddToBuffer(GameObject target, Sprite sprite, Sprite blinkSprite, DefaultSprites ds, DefaultSprites bs, float blinkTime)
    // {
    //     objectBuffer.Add(target);
    //     spriteBuffer.Add(sprite);
    //     blinkSpriteBuffer.Add(blinkSprite);
    //     enumBuffer.Add(ds);
    //     blinkEnumBuffer.Add(bs);
    //     blinkTimeBuffer.Add(blinkTime);
    // }

    public static void RemoveTracker(GameObject toRemove) {
        if (_instance == null){
            removeBuffer.Add(toRemove);
            return;
        }
        for(int i = 0; i < _instance.targets.Count; i++) {
            if (_instance.targets[i].target == toRemove) {
                Destroy(_instance.targets[i].gameObject);
                _instance.targets.RemoveAt(i);
            }
        }
    }
}
