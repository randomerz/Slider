using UnityEngine;
using UnityEngine.UI;

public class UIPowerTracker : FlashWhiteImage
{
    [SerializeField] private Image poweredImage;
    [SerializeField] private ElectricalNode myNode;
    [SerializeField] private bool enabledInFactoryPast;

    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();

        if (myNode == null)
        {
            Debug.LogError("UIPowerTracker requires node to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        UpdatePoweredImageEnabled();
    }

    private void UpdatePoweredImageEnabled()
    {
        bool nodeOnDisabledButton = button != null && !button.TileIsActive;
        if (SGrid.Current is FactoryGrid)
        {
            poweredImage.enabled = myNode.Powered && !nodeOnDisabledButton && (enabledInFactoryPast == FactoryGrid.PlayerInPast);
        }
        else
        {
            poweredImage.enabled = myNode.Powered && !nodeOnDisabledButton;
        }
    }
}
