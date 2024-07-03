using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryProp : MonoBehaviour
{
    //[SerializeField] private SpriteSwapper spriteSwapper;

    public bool isDecorativeAndOn;

    public bool IsGateEnabled { get; private set; }
    public bool IsDiodeEnabled { get; private set; }

    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite enabledSprite;
    [SerializeField] private Sprite poweredSprite;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<ParticleSystem> particlesBurst = new List<ParticleSystem>();
    [SerializeField] private List<ParticleSystem> particlesConducting = new List<ParticleSystem>();

    private void Start() 
    {
        if (isDecorativeAndOn)
        {
            // SetGateEnabled(true);
            IsGateEnabled = true;
            IsDiodeEnabled = true;
            SetDiodeEnabled(true);
        }
    }

    public void SetGateEnabled(bool value)
    {
        if (value && !IsGateEnabled)
        {
            foreach (ParticleSystem ps in particlesBurst) 
            {
                ps.Play();
            }
        }

        IsGateEnabled = value;
        UpdateSprite();
    }

    public void SetDiodeEnabled(bool value)
    {
        if (value)
        {
            foreach (ParticleSystem ps in particlesConducting) 
            {
                ps.Play();
            }

            if (!IsDiodeEnabled)
            {
                foreach (ParticleSystem ps in particlesBurst) 
                {
                    ps.Play();
                }
            }
        }
        else
        {
            foreach (ParticleSystem ps in particlesConducting) 
            {
                ps.Stop();
            }
        }

        IsDiodeEnabled = value;
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (IsDiodeEnabled)
        {
            spriteRenderer.sprite = poweredSprite;
        }
        else if (IsGateEnabled)
        {
            spriteRenderer.sprite = enabledSprite;
        }
        else
        {
            spriteRenderer.sprite = offSprite;
        }
    }
}
