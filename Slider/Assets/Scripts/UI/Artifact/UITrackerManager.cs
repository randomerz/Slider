using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrackerManager : MonoBehaviour
{
    protected static UITrackerManager _instance;
    
    protected Vector2 center = new Vector2(17, 17);
    protected Vector2 position;
    protected Vector2 offset;
    protected ArtifactTileButton currentButton;
    protected STile currentTile;
    protected float scale = 27f/17f;
    protected float centerScale = 37f/17f;
    
    public UIArtifactMenus uiArtifactMenus;
    public GameObject artifactPanel;
    public GameObject uiTrackerPrefab;
    public List<UITracker> targets = new List<UITracker>();
    
    // Buffers for when called before initialization
    private static List<GameObject> objectBuffer = new List<GameObject>();
    private static List<Sprite> spriteBuffer = new List<Sprite>();
    private static List<Sprite> blinkSpriteBuffer = new List<Sprite>();
    private static List<DefaultSprites> enumBuffer = new List<DefaultSprites>();
    private static List<DefaultSprites> blinkEnumBuffer = new List<DefaultSprites>();
    private static List<float> blinkTimeBuffer = new List<float>();

    private static List<GameObject> removeBuffer = new List<GameObject>();

    [Header("Default Pins")]
    public Sprite empty;
    public Sprite circle1;
    public Sprite circle2;
    public Sprite circleEmpty;
    public Sprite pin;
    public Sprite exclamation;

    public enum DefaultSprites {
        none,
        circle1,
        circle2,
        circleEmpty,
        pin,
        exclamation,
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
        if (objectBuffer.Count > 0) {
            for(int i = 0; i < objectBuffer.Count; i++) {
                if (spriteBuffer[i] != null)
                {
                    AddNewTracker(objectBuffer[i], spriteBuffer[i], blinkSpriteBuffer[i], blinkTimeBuffer[i]);
                }
                else
                {
                    AddNewTracker(objectBuffer[i], enumBuffer[i], blinkEnumBuffer[i], blinkTimeBuffer[i]);
                }
                
            }
            objectBuffer.Clear();
            spriteBuffer.Clear();
            blinkSpriteBuffer.Clear();
            enumBuffer.Clear();
            blinkEnumBuffer.Clear();
            blinkTimeBuffer.Clear();
        }
        if (removeBuffer.Count > 0) {
            for(int i = 0; i < objectBuffer.Count; i++) {
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
            t.image.rectTransform.SetParent(currentButton.transform.Find("Image").GetComponent<RectTransform>());
            
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
        offset = new Vector3(Mathf.Clamp(offset.x, -62.5f, 62.5f), Mathf.Clamp(offset.y, -57.5f, 57.5f));
    }

    protected virtual void CalculateOffset() 
    {
        offset = (position - (Vector2)currentTile.transform.position) * scale;
    }


    public static void AddNewTracker(GameObject target, DefaultSprites sprite=DefaultSprites.circle1, DefaultSprites blinkSprite=DefaultSprites.none, float blinkTime=-1) {
        if (_instance == null){
            AddToBuffer(target, null, null, sprite, blinkSprite, blinkTime);
            return;
        }

        AddNewTracker(target, EnumToSprite(sprite), EnumToSprite(blinkSprite), blinkTime);
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

        }
        Debug.LogWarning("Couldn't find pin!");
        return null;
    }
    
    public static void AddNewTracker(GameObject target, Sprite sprite, Sprite blinkSprite=null, float blinkTime=-1) {
        if (_instance == null){
            AddToBuffer(target, sprite, blinkSprite, DefaultSprites.none, DefaultSprites.none, blinkTime);
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

        _instance.targets.Add(uiTracker);
    }

    private static void AddToBuffer(GameObject target, Sprite sprite, Sprite blinkSprite, DefaultSprites ds, DefaultSprites bs, float blinkTime)
    {
        objectBuffer.Add(target);
        spriteBuffer.Add(sprite);
        blinkSpriteBuffer.Add(blinkSprite);
        enumBuffer.Add(ds);
        blinkEnumBuffer.Add(bs);
        blinkTimeBuffer.Add(blinkTime);
    }

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
