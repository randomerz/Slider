using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJungleHutTracker : FlashWhiteImage
{
    [SerializeField] private Image mainHutImage;
    [SerializeField] private List<Image> directionImages;
    [SerializeField] private List<Sprite> mainHutSprites;
    [SerializeField] private Box myHut;

    private ArtifactTileButton button;

    private readonly Direction[] DIRECTIONS = 
    {
        Direction.LEFT,
        Direction.UP,
        Direction.RIGHT,
        Direction.DOWN,
    };

    protected override void Awake()
    {
        base.Awake();

        if (myHut == null)
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
        mainHutImage.sprite = mainHutSprites[myHut.currentShapeIndex];

        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            directionImages[i].enabled = (
                !nodeOnDisabledButton &&
                myHut.currentDirection == DIRECTIONS[i]
            );

            if (myHut is DoubleSign)
            {
                directionImages[i].enabled = (
                    !nodeOnDisabledButton &&
                    (
                        myHut.currentDirection == DIRECTIONS[i] ||
                        (myHut as DoubleSign).secondCurrentDirection == DIRECTIONS[i]
                    )
                );
            }
        }
    }
}
