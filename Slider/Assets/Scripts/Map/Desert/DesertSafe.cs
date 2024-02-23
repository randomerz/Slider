using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertSafe : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mirageSpriteRenderer;
    [SerializeField] private Sprite meltedMirageSprite;

    [SerializeField] private float timeToBeLaseredForMelt;
    [SerializeField] private DinoLasersManager dinoLasersManager;

    private bool currentlyLasered = false;
    private bool melted = false;

    private bool laseredLastUpdate = false;
    private float laseredStartTime;

    private void Start() 
    {
        if (SaveSystem.Current.GetBool("desertSafeMelted"))
        {
            MeltSafe();
        }
    }

    public void OnLasered()
    {
        //Debug.Log("Safe lasered!");
        currentlyLasered = true;
    }

    public void OnUnLasered()
    {
       // Debug.Log("Safe Unlasered!");
        currentlyLasered = false;
    }

    private void Update()
    {
        if (!melted)
        {
            if (currentlyLasered && !laseredLastUpdate) 
            {
                laseredLastUpdate = true;
                laseredStartTime = Time.time;
            }
            else if (currentlyLasered && laseredLastUpdate)
            {
                if (Time.time - laseredStartTime > timeToBeLaseredForMelt)
                {
                    MeltSafe();
                    return;
                }
            }
            else if (!currentlyLasered && laseredLastUpdate)
            {
                laseredLastUpdate = false;
            }
        }
    }

    private void MeltSafe()
    {
        //Debug.Log("Safe Melted!");
        melted = true;

        mirageSpriteRenderer.sprite = meltedMirageSprite;

        SaveSystem.Current.SetBool("desertSafeMelted", true);

        dinoLasersManager.RemoveAllLasersPermanently();
        // MirageSTileManager.GetInstance().DisableMirage();
    }
}
