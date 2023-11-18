using UnityEngine;
using UnityEngine.UI;

public class UIJunctionTracker : FlashWhiteImage
{
    [SerializeField] private Image image;
    [SerializeField] private MinecartJunctionNode myNode;
    [SerializeField] private Sprite poweredOffSprite;
    [SerializeField] private Sprite poweredOnSprite;
    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();
        if (myNode == null)
        {
            Debug.LogError("UIJunctionTracker requires node to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;
        image.enabled = !nodeOnDisabledButton;
        image.sprite = myNode.Powered ? poweredOnSprite : poweredOffSprite;
    }  
}
