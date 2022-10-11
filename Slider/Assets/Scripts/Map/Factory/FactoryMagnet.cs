using UnityEngine;

public class FactoryMagnet : MonoBehaviour 
{
    public SpriteRenderer poweredSprite;
    public BobAnimation bobAnimation;
    public ParticleSystem topParticles;
    public ParticleSystem bottomParticles;

    public FactoryMagnet magnetPair;

    public bool isPowered;

    private void Start() {
        SetPowered(isPowered);
    }

    private void OnEnable() {
        SGridAnimator.OnSTileMoveEnd += CheckConnected;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckConnected;
    }

    public void SetPowered(bool value)
    {
        isPowered = value;

        poweredSprite.enabled = isPowered;
        bobAnimation.enabled = isPowered;
        
        if (isPowered)
        {
            topParticles.Play();
            bottomParticles.Play();
        }
        else
        {
            topParticles.Stop();
            bottomParticles.Stop();
            bobAnimation.ResetPosition();
        }
        
    }

    public void SetConnected(bool value)
    {
        SetBobSpeed(value ? 0.125f : 0.25f);
    }

    public void SetBobSpeed(float value)
    {
        bobAnimation.totalLength = value;
        if (bobAnimation.initOffset != 0)
            bobAnimation.initOffset = value / 2;
    }

    public void CheckConnected(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile.islandId == 2 && e.stile.x == 0 && e.stile.y == 0)
        {
            // check if moving tile 2 to bottom left
            if (isPowered && magnetPair.isPowered)
            {
                SetConnected(true);
                magnetPair.SetConnected(true);
                AudioManager.Play("Glass Clink");
                return;
            }
        }
        SetConnected(false);
        magnetPair.SetConnected(false);
    }
}