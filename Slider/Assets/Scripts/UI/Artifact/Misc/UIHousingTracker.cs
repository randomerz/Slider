using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHousingTracker : FlashWhiteImage
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject uiGameObject;
    [SerializeField] private bool displayInHouse = true;
    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();

        button = GetComponentInParent<ArtifactTileButton>();
    }

    private void Update()
    {
        UpdateTracker();
    }

    private void UpdateTracker()
    {
        bool nodeOnEnabledButton = button != null && button.TileIsActive;
        bool houseCondition = Player.GetIsInHouse() == displayInHouse;

        if (image != null)
        {
            image.enabled = nodeOnEnabledButton && houseCondition;
        }
        if (uiGameObject != null)
        {
            uiGameObject.SetActive(nodeOnEnabledButton && houseCondition);
        }
    }
}
