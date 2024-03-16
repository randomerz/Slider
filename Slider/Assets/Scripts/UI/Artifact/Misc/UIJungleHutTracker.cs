using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJungleHutTracker : FlashWhiteImage
{
    [SerializeField] private Image mainHutImage;
    [SerializeField] private List<Image> directionImages;
    [SerializeField] private List<Sprite> mainHutSprites;
    [SerializeField] private JungleSpawner mySpawner;

    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();

        if (mySpawner == null)
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
        mainHutImage.sprite = mainHutSprites[mySpawner.CurrentShapeIndex];

        for (int i = 0; i < DirectionUtil.Directions.Length; i++)
        {
            directionImages[i].enabled = (
                !nodeOnDisabledButton &&
                mySpawner.CurrentDirection == DirectionUtil.Directions[i]
            );
        }
    }
}
