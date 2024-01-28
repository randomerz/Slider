using System;
using UnityEngine;

public class FactoryMagnet : MonoBehaviour 
{
    public SpriteRenderer poweredSprite;
    public BobAnimation bobAnimation;
    public ParticleSystem topParticles;
    public ParticleSystem bottomParticles;
    public FactoryMagnet magnetPair;
    public GameObject uiIcon;

    [SerializeField] private bool powerOnStart;

    private bool _isPoweredWithoutBlackout;
    private bool _lastPoweredState = false;

    private void Start() {
        SetPowered(powerOnStart);
    }

    private void OnEnable() {
        SGridAnimator.OnSTileMoveEnd += CheckConnected;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= CheckConnected;
    }

    private void Update()
    {
        if (GetPowered() != _lastPoweredState)
        {
            UpdatePoweredState();
        }

        _lastPoweredState = GetPowered();
    }

    public bool GetPowered()
    {
        return _isPoweredWithoutBlackout && !PowerCrystal.Blackout;
    }

    public void SetPowered(bool value)
    {
        _isPoweredWithoutBlackout = value;
        UpdatePoweredState();
    }

    private void UpdatePoweredState()
    {
        bool isPowered = GetPowered();
        poweredSprite.enabled = isPowered;
        bobAnimation.enabled = isPowered;
        if(uiIcon)
            uiIcon.SetActive(isPowered);
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
        if (magnetPair == null)
        {
            return;
        }

        if (e.stile.islandId == 2 && e.stile.x == 0 && e.stile.y == 0)
        {
            // check if moving tile 2 to bottom left
            if (GetPowered() && magnetPair.GetPowered())
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