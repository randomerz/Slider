using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJungleDuplicatorTracker : FlashWhiteImage
{
    [SerializeField] private Image mainHutImage;
    [SerializeField] private List<Image> directionImages;
    [SerializeField] private JungleDuplicator myDuplicator;

    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();

        if (myDuplicator == null)
        {
            Debug.LogError("UIJungleHutTracker requires hut to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        UpdateHut();
    }

    private void UpdateHut()
    {
        bool nodeOnDisabledButton = (button != null && !button.TileIsActive) || !Player.GetIsInHouse();

        mainHutImage.enabled = !nodeOnDisabledButton;

        for (int i = 0; i < DirectionUtil.Directions.Length; i++)
        {
            directionImages[i].enabled = (
                !nodeOnDisabledButton &&
                (
                    myDuplicator.CurrentDirection == DirectionUtil.Directions[i] ||
                    myDuplicator.CurrentAlternateDirection == DirectionUtil.Directions[i]
                )
            );
        }
    }
}
