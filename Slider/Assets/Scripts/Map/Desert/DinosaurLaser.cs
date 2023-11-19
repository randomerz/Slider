using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineSmoothPath;

public class DinosaurLaser : MonoBehaviour
{
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
    //[SerializeField] private /*static*/bool neverActivatedBefore = true;
    private const string desertDinoLaserActivatedAlready = "desertDinoLaserActivatedAlready";

    [SerializeField] private GameObject laserGameObject;

    private bool dinoConnected;
    public void CheckIsDinoConnected(Condition c) { c.SetSpec(dinoConnected); }

    private void Start()
    {
        //laserGameObject = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
        SGridAnimator.OnSTileMoveStart += OnMoveStart;
        DesertArtifact.MirageDisappeared += OnMirageDisappeared;
        CheckEnableLaser();
    }

    private void LateUpdate()
    {
        moveEndWasCheckedThisFrame = false;
        moveStartWasCheckedThisFrame = false;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnMoveEnd;
        SGridAnimator.OnSTileMoveStart -= OnMoveStart;
        DesertArtifact.MirageDisappeared -= OnMirageDisappeared;
    }


    private void OnMoveEnd(object sender, System.EventArgs e)
    {
        if (!moveEndWasCheckedThisFrame)
        {
            moveEndWasCheckedThisFrame = true;
            CheckEnableLaser();
        }
    }

    private void OnMoveStart(object sender, System.EventArgs e)
    {
        if (!moveStartWasCheckedThisFrame)
        {
            moveStartWasCheckedThisFrame = true;
            CheckDisableLaser();
        }
    }

    private void OnMirageDisappeared(object sender, System.EventArgs e)
    {
            Debug.Log("On Mirage Disaapear");
        CheckDisableLaser();
    }

    public void FirstTimeActivate()
    {
        StartCoroutine(PlayFirstTimeActivationAnimation());
    }

    private void CheckEnableLaser()
    {
        /*
        bool activatedPreviously =  SaveSystem.Current.GetBool(desertDinoLaserActivatedAlready);

        //first activation must be from mirage tile laser
        //if (!activatedPreviously && !(tileId == 'd')) { return; }

        bool connected = IsConnectedToDinoButt();

        if (activatedPreviously)
        {
           StartCoroutine(ActivateHalvesInTime(true, 0.5f));
        }*/
        bool activatedPreviously = SaveSystem.Current.GetBool(desertDinoLaserActivatedAlready);
        //If it has never been activated we don't need to enable it
        if (!activatedPreviously) 
        {
            //first activation must be from mirage tile laser
            if (!(tileId == 'd'))
            {
                return;
            }
            IsConnectedToDinoButt();
            return;
        }

        bool connected = IsConnectedToDinoButt();
        if (connected)
        {
            StartCoroutine(ActivateHalvesInTime(true, 0.5f));
        }
        
            /*
            if (!activatedPreviously)
            {
                if(CheckGrid.contains(DesertGrid.GetGridString(), "[7g]d"))
                {
                    //StartCoroutine(PlayFirstTimeActivationAnimation());
                    dinoConnected = true;
                }
            }
            else
            {
                if(CheckGrid.contains(SGrid.GetGridString(), "[7g]" + tileId))
                {

                }
                else
                {
                    dinoConnected = false;
                }
            }*/

            /*
    string gridString = SGrid.GetGridString();
    Debug.Log(gridString);
    if (CheckGrid.contains(gridString, "47"))
    {
        Debug.Log("LASER");
    }
    else if (CheckGrid.contains(gridString, "#7") || CheckGrid.contains(gridString, "4#"))
    {

    }
    SpriteRenderer dinoButt = GetDinoButtOnTheLeft();
    if (dinoButt != null)
    {
        dinoConnected = true;
    }
    else
    {
        dinoConnected = false;
    }


    if (!dinoConnected && laserOn) //no dino butt on the left
    {
        Debug.Log("turn off");
        //dinoButt.sprite = leftHalfLaserOffSprite;
        //rightHalfSpriteRenderer.sprite = rightHalfLaserOffSprite;
        if (!neverActivatedBefore)
        {
            StartCoroutine(ActivateHalvesInTime(false, 0.01f, currentDinoButt, rightHalfSpriteRenderer));
        }
        currentDinoButt = null;
        laserOn = false;
        //SaveSystem.Current.SetBool("desertDinoConnected", false);
    } 
    else if (dinoConnected && !laserOn)
    {
        Debug.Log("turn on");

        if (!neverActivatedBefore)
        {
            StartCoroutine(ActivateHalvesInTime(true, 0.5f, dinoButt, rightHalfSpriteRenderer));
        }
        currentDinoButt = dinoButt;
        laserOn = true;
        //SaveSystem.Current.SetBool("desertDinoConnected", true); 
    }
    lastTimeChecked = Time.time;*/
        }

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
    }

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
    }

    private IEnumerator ActivateHalvesInTime(bool on, float time)
    {
        SpriteRenderer leftHalf = connectedDinoButtSpriteRenderer;
        SpriteRenderer rightHalf = rightHalfSpriteRenderer;

        yield return new WaitForSeconds(time);
        
        if (on)
        {
            leftHalf.sprite = leftHalfLaserOnSprite;
            rightHalf.sprite = rightHalfLaserOnSprite;
            laserGameObject.SetActive(true);
            laserOn = true;
        }
        else
        {
            leftHalf.sprite = leftHalfLaserOffSprite;
            rightHalf.sprite = rightHalfLaserOffSprite;
            laserGameObject.SetActive(false);
            laserOn = false;
        }
    }

    private IEnumerator PlayFirstTimeActivationAnimation()
    {
        if (!(tileId == 'd')) { yield break; } //Only play this on the mirage laser

        Debug.Log("first time animation");
        //cameraDolly.path.m_Waypoints;
        /*
        Waypoint waypoint = new Waypoint();
        waypoint.position = transform.position - new Vector3(5, 0, 0);
        Waypoint[] waypoints = { waypoint };
        cameraDolly.path.m_Waypoints = waypoints;

        cameraDolly.StartTrack();
        */
        yield return new WaitForSeconds(2);

        firstTimeActivationAnimation.SetActive(true);
        //firstTimeActivationAnimation.GetComponent<Animator>().Play();
        yield return new WaitForSeconds(2.167f);// We love magic waits

        firstTimeActivationAnimation.SetActive(false);
        Destroy(firstTimeActivationAnimation);

        GetComponent<SpriteRenderer>().enabled = true; //now using post laser dino head (broken skull)
        //neverActivatedBefore = false;
        SaveSystem.Current.SetBool("desertDinoLaserActivatedAlready", true);

        StartCoroutine(ActivateHalvesInTime(true, 0f));
    }
}
