using UnityEngine;
using UnityEngine.UI;

public class UIPowerTracker : FlashWhiteUI
{
    [SerializeField] private Image poweredImage;
    [SerializeField] private ElectricalNode myNode;

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
            poweredImage.enabled = myNode.Powered && !nodeOnDisabledButton && !FactoryGrid.PlayerInPast;
        }
        else
        {
            poweredImage.enabled = myNode.Powered && !nodeOnDisabledButton;
        }
    }
}
