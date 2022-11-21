using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryTab : ArtifactTab
{
    [SerializeField] private TimedGate gate;
    [SerializeField] private List<Sprite> countdownSprite;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failureSprite;
    [SerializeField] private Sprite blinkSprite;
    [SerializeField] private Image image;

    private List<Sprite> _allSprites = new List<Sprite>(11);

    public bool Activated { get { return gate.GateActive; } }

    private void Awake()
    {
        _allSprites.Clear();
        _allSprites.AddRange(countdownSprite);
        _allSprites.Add(successSprite);
        _allSprites.Add(failureSprite);
        _allSprites.Add(blinkSprite);
    }

    private void Update()
    {
        int i = gate.GetTabSpriteIndex();
        if (i >= 0)
        {
            image.sprite = _allSprites[i];
        }
    }

    public override void SetIsVisible(bool value)
    {
        isActive = !value ? false : gate.GateActive && !gate.Powered;
        isVisible = isActive;
        if (!value && gameObject.activeInHierarchy)
        {
            StartCoroutine(SetVisibleThenDisable());
        }
        else
        {
            gameObject.SetActive(isActive);
            UpdateVisibility();
        }
    }
}
