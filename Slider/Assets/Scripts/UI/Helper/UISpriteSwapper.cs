using UnityEngine;
using UnityEngine.UI;

public class UISpriteSwapper : FlashWhiteImage
{
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;
    [SerializeField] private Image image;
    private ArtifactTileButton button;

    protected override void Awake()
    {
        base.Awake();
        button = GetComponentInParent<ArtifactTileButton>();
    }
    
    private void Update() => image.enabled = button.TileIsActive;

    public void TurnOn() => image.sprite = on;

    public void TurnOff() => image.sprite = off;
}
