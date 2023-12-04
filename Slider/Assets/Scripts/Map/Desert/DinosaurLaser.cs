using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineSmoothPath;

public class DinosaurLaser : MonoBehaviour
{
    /*
    [SerializeField] private CameraDolly cameraDolly;
    [SerializeField] private GameObject firstTimeActivationAnimation;
    [SerializeField] private SpriteRenderer[] leftHalfSpriteRenderers; //0 is the normal one, 1 is the mirage one
    [SerializeField] private SpriteRenderer rightHalfSpriteRenderer;

    [SerializeField] private Sprite leftHalfLaserOffSprite;
    [SerializeField] private Sprite leftHalfLaserOnSprite;
    [SerializeField] private Sprite rightHalfLaserOffSprite;
    [SerializeField] private Sprite rightHalfLaserOnSprite;

    private SpriteRenderer connectedDinoButtSpriteRenderer;
    [SerializeField] private char tileId;
    private static bool moveEndWasCheckedThisFrame = false;
    private static bool moveStartWasCheckedThisFrame = false;

    [SerializeField] private bool laserOn = false;

    private const string desertDinoLaserActivatedAlready = "desertDinoLaserActivatedAlready";
    */
    //[SerializeField] private GameObject laserGameObject;
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
    }

    //private bool dinoConnected;
    //public void CheckIsDinoConnected(Condition c) { c.SetSpec(dinoConnected); }

    /*
    private void CheckDisableLaser()
    {
        bool activatedPreviously = SaveSystem.Current.GetBool(desertDinoLaserActivatedAlready);
        //If it has never been activated we don't need to disable it
        if (!activatedPreviously) { return; }

        bool connected = IsConnectedToDinoButt();
        if (!connected)
        {
            StartCoroutine(ActivateHalvesInTime(false, 0.01f));
        }
    }*/
    /*
    private bool IsConnectedToDinoButt()
    {
        //update our variable whenever we check
        //dinoConnected = CheckGrid.contains(SGrid.GetGridString(), "[7g]" + tileId);
        string gridString = DesertGrid.GetGridString();
        Debug.Log(gridString);

        if (CheckGrid.contains(gridString, "7" + tileId))
        {
            connectedDinoButtSpriteRenderer = leftHalfSpriteRenderers[0];
            dinoConnected = true;
        }
        else if (CheckGrid.contains(gridString, "g" + tileId))
        {
            connectedDinoButtSpriteRenderer = leftHalfSpriteRenderers[1];
            dinoConnected = true;
        }
        else
        {
            connectedDinoButtSpriteRenderer = null;
            dinoConnected = false;
        }
        return dinoConnected;
    }*/
}
