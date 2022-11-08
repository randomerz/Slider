using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryServerPropLights : MonoBehaviour
{
    private class LightData 
    {
        public bool enabled;
        public SpriteRenderer spriteRenderer;
        public float timeTillSwap;

        public LightData(SpriteRenderer spriteRenderer) {
            this.spriteRenderer = spriteRenderer;
            timeTillSwap = Random.Range(0, MAX_BLINK_COUNTDOWN);
        }

        public void Update()
        {
            timeTillSwap -= Time.deltaTime;
            if (timeTillSwap <= 0)
            {
                Swap();
            }
        }

        private void Swap() 
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            timeTillSwap = Random.Range(0, MAX_BLINK_COUNTDOWN);
        }
    }

    public ElectricalNode power;
    public bool lightsAlwaysOn;
    public List<SpriteRenderer> lights;
    private List<LightData> lightData = new List<LightData>();
    private const float MAX_BLINK_COUNTDOWN = 5;

    void Awake()
    {
        foreach (SpriteRenderer s in lights)
            lightData.Add(new LightData(s));
    }

    private void OnEnable() 
    {
        SetLights(power.Powered || lightsAlwaysOn);
    }

    void Update()
    {
        // It's all or nothing!
        if (!lightData[0].enabled)
            return;

        foreach (LightData l in lightData)
            l.Update();
    }


    public void SetLights(bool value)
    {
        foreach (LightData l in lightData)
            SetLight(l, value);
    }

    private void SetLight(LightData light, bool value)
    {
        light.enabled = value;
        light.spriteRenderer.enabled = value;
        light.timeTillSwap = Random.Range(0, MAX_BLINK_COUNTDOWN);
    }
}
