using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJungleSignTracker : FlashWhiteImage
{
    [SerializeField] private Image mainSignImage;
    [SerializeField] private List<Sprite> directionSprites;
    [SerializeField] private JungleSign mySign;

    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();

        if (mySign == null)
        {
            Debug.LogError("UIJungleSignTracker requires sign to track");
        }
        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        UpdateSign();
    }

    private void UpdateSign()
    {
        bool nodeOnDisabledButton = (button != null && !button.TileIsActive) || !Player.GetIsInHouse();

        mainSignImage.enabled = !nodeOnDisabledButton;

        for (int i = 0; i < DirectionUtil.Directions.Length; i++)
        {
            if (mySign.CurrentDirection == DirectionUtil.Directions[i])
            {
                mainSignImage.sprite = directionSprites[i];
                break;
            }
        }
    }
}
