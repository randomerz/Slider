using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrackerManager : MonoBehaviour
{
    private static UITrackerManager _instance;
    
    private Vector2 center = new Vector2(17, 17);
    private Vector2 position;
    private Vector2 offset;
    private ArtifactTileButton currentButton;
    private STile currentTile;
    private float scale = 26f/17f;
    private float centerScale = 36f/17f;
    public UIArtifact artifact;
    public GameObject artifactPanel;
    public GameObject uiTrackerPrefab;
    public List<UITracker> targets;


    void Awake(){
        _instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int x = 0; x < targets.Count; x++){
            if (targets[x].target == null){
                Destroy(targets[x].gameObject);
                targets.RemoveAt(x);
            }
        }

        foreach(UITracker x in targets){
            position = x.getPosition();
            currentTile = x.GetSTile();

            //S: When target is not on a tile
            if(currentTile == null){
                x.image.rectTransform.SetParent(artifactPanel.GetComponent<RectTransform>());
                offset = (position - center) * centerScale;

                x.image.rectTransform.anchoredPosition = offset;

                continue;
            }

            currentButton = artifact.GetButton(currentTile.islandId);

            offset = (position - (Vector2)currentTile.transform.position) * scale;
            x.image.rectTransform.SetParent(currentButton.GetComponent<RectTransform>());
            
            if(x.GetIsInHouse())
            {
                x.image.rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                x.image.rectTransform.anchoredPosition = offset;
            }
        }
    }
    
    public static void addNewTracker(GameObject target, Sprite sprite){
        GameObject tracker = GameObject.Instantiate(_instance.uiTrackerPrefab, _instance.transform);
        UITracker uiTracker = tracker.GetComponent<UITracker>();
        uiTracker.target = target;
        uiTracker.image.sprite = sprite;
        _instance.targets.Add(uiTracker);
    }
}
