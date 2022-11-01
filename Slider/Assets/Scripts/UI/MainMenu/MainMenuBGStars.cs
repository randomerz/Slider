using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBGStars : MonoBehaviour
{
    private class LightData 
    {
        public bool enabled;
        public SpriteSwapper spriteSwapper;
        public float timeTillSwap;

        public LightData(SpriteSwapper spriteSwapper) {
            this.spriteSwapper = spriteSwapper;
            timeTillSwap = Random.Range(0, MAX_TIME_ON);
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
            enabled = !enabled;
            if (enabled) spriteSwapper.TurnOn();
            else spriteSwapper.TurnOff();
            timeTillSwap = enabled ? Random.Range(MAX_TIME_ON / 2f, MAX_TIME_ON)
                                   : Random.Range(MAX_TIME_OFF / 2f, MAX_TIME_OFF);
        }
    }

    public List<SpriteSwapper> starBlips = new List<SpriteSwapper>();
    
    private List<LightData> stars = new List<LightData>();
    private const float MAX_TIME_ON = 50;
    private const float MAX_TIME_OFF = 1;



    void Awake()
    {
        foreach (SpriteSwapper i in starBlips)
            stars.Add(new LightData(i));
    }

    void Update()
    {
        foreach (LightData l in stars)
            l.Update();
    }
}
