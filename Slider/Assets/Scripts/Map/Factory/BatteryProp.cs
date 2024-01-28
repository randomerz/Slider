using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryProp : MonoBehaviour
{
    //[SerializeField] private SpriteSwapper spriteSwapper;

    public bool isDecorativeAndOn;

    private bool isGateEnabled;
    private bool isDiodeEnabled;

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
            isGateEnabled = true;
            isDiodeEnabled = true;
            SetDiodeEnabled(true);
        }
    }

    public void SetGateEnabled(bool value)
    {
        if (value && !isGateEnabled)
        {
            foreach (ParticleSystem ps in particlesBurst) 
            {
                ps.Play();
            }
        }

        isGateEnabled = value;
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

            if (!isDiodeEnabled)
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

        isDiodeEnabled = value;
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (isDiodeEnabled)
        {
            spriteRenderer.sprite = poweredSprite;
        }
        else if (isGateEnabled)
        {
            spriteRenderer.sprite = enabledSprite;
        }
        else
        {
            spriteRenderer.sprite = offSprite;
        }
    }
}
