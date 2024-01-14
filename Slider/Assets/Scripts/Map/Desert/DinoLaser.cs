using UnityEngine;

public class DinoLaser : MonoBehaviour
{
    [SerializeField] private MagiLaser laser;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void EnableSpriteRenderer(bool on)
    {
        spriteRenderer.enabled = on;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void EnableLaser(bool on)
    {
        laser.SetEnabled(on);
        ArtifactTBPluginLaser.UpdateSpritesFromSource();
    }
}
