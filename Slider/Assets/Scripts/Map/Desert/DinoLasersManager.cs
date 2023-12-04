using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DinoLasersManager : MonoBehaviour
{
    private bool debugSkipFezziwigActivation = true;

    [SerializeField] private Sprite leftHalfLaserOffSprite;
    [SerializeField] private Sprite leftHalfLaserOnSprite;
    [SerializeField] private Sprite rightHalfLaserOffSprite;
    [SerializeField] private Sprite rightHalfLaserOnSprite;

    
    [SerializeField] private DinosaurLaser[] dinoLasers; //0 is normal tile 4 laser, 1 is mirage tile 4 laser 
    [SerializeField] private SpriteRenderer[] dinoButtSpriteRenderers; //0 is normal tile 7 butt, 1 is mirage tile 7 butt 

    private bool moveEndWasCheckedThisFrame = false;
    private bool moveStartWasCheckedThisFrame = false;

    [SerializeField] private GameObject firstTimeActivationAnimation;
    private const string desertDinoLaserActivatedAlready = "desertDinoLaserActivatedAlready";

    private bool canFirstTimeActivate = false;
    public void CheckCanFirstTimeActivate(Condition c) 
    {
        c.SetSpec(canFirstTimeActivate); 
    }

    private void UpdateCanFirstTimeActivate(object sender, System.EventArgs e)
    {
        bool activatedPreviously = SaveSystem.Current.GetBool(desertDinoLaserActivatedAlready);

        string gridString = DesertGrid.GetGridString();
        Debug.Log("FIRST TIME ACTIVATE LASER Checking " + gridString);

        if (!activatedPreviously && (CheckGrid.contains(gridString, "7d") || CheckGrid.contains(gridString, "gd"))) //normal tail | mirage head
        {
            canFirstTimeActivate = true;
        }
    }

    private void Start()
    {
        if (debugSkipFezziwigActivation)
        {
            Destroy(firstTimeActivationAnimation);

            foreach (DinosaurLaser dinoLaser in dinoLasers)
            {
                dinoLaser.EnableSpriteRenderer(true);//now using post laser dino head (broken skull)
            }

            SaveSystem.Current.SetBool("desertDinoLaserActivatedAlready", true);

            SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
            DesertArtifact.MirageDisappeared += OnMirageDisappeared;
        }
        else
        {
            SGridAnimator.OnSTileMoveEnd += UpdateCanFirstTimeActivate;
            DesertArtifact.MirageDisappeared += UpdateCanFirstTimeActivate;
        }
    }

    private void LateUpdate()
    {
        moveEndWasCheckedThisFrame = false;
        moveStartWasCheckedThisFrame = false;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnMoveEnd;
        //SGridAnimator.OnSTileMoveStart -= OnMoveStart;
        DesertArtifact.MirageDisappeared -= OnMirageDisappeared;
    }


    private void OnMoveEnd(object sender, System.EventArgs e)
    {
        if (!moveEndWasCheckedThisFrame)
        {
            moveEndWasCheckedThisFrame = true;
            CheckDisableLasers();
            CheckEnableLasers();
        }
    }

    private void OnMoveStart(object sender, System.EventArgs e)
    {
        if (!moveStartWasCheckedThisFrame)
        {
            moveStartWasCheckedThisFrame = true;
            //CheckDisableLasers();
        }
    }

    private void OnMirageDisappeared(object sender, System.EventArgs e)
    {
        Debug.Log("On Mirage Disaapear");
        CheckDisableLasers();
    }

    private void CheckEnableLasers()
    {
        string gridString = DesertGrid.GetGridString();
        Debug.Log("ENABLE LASERS Checking " + gridString);

        if (CheckGrid.contains(gridString, "74")) //normal tail | normal head
        {
            ActivateButt(true, dinoButtSpriteRenderers[0]);
            ActivateHead(true, dinoLasers[0]);
        }

        if (CheckGrid.contains(gridString, "g4")) //mirage tail | normal head
        {
            ActivateButt(true, dinoButtSpriteRenderers[1]);
            ActivateHead(true, dinoLasers[0]);
        }

        if (CheckGrid.contains(gridString, "7d")) //normal tail | mirage head
        {
            ActivateButt(true, dinoButtSpriteRenderers[0]);
            ActivateHead(true, dinoLasers[1]);
        }

        if (CheckGrid.contains(gridString, "gd")) //mirage tail | mirage head
        {
            ActivateButt(true, dinoButtSpriteRenderers[1]);
            ActivateHead(true, dinoLasers[1]);
        }
    }

    private void CheckDisableLasers()
    {
        string gridString = DesertGrid.GetGridString();
        Debug.Log("DISABLE LASERS Checking " + gridString);

        bool normalButtConnected = false;
        bool normalHeadConnected = false;
        bool mirageButtConnected = false;
        bool mirageHeadConnected = false;

        if (CheckGrid.contains(gridString, "74")) //normal tail | normal head
        {
            normalButtConnected = true;
            normalHeadConnected = true;
        }

        if (CheckGrid.contains(gridString, "g4")) //mirage tail | normal head
        {
            mirageButtConnected = true;
            normalHeadConnected = true;
        }

        if (CheckGrid.contains(gridString, "7d")) //normal tail | mirage head
        {
            normalButtConnected = true;
            mirageHeadConnected = true;
        }

        if (CheckGrid.contains(gridString, "gd")) //mirage tail | mirage head
        {
            mirageButtConnected = true;
            mirageHeadConnected = true;
        }

        if (!normalButtConnected) { ActivateButt(false, dinoButtSpriteRenderers[0]); }
        if (!normalHeadConnected) { ActivateHead(false, dinoLasers[0]); }
        if (!mirageButtConnected) { ActivateButt(false, dinoButtSpriteRenderers[1]); }
        if (!mirageHeadConnected) { ActivateHead(false, dinoLasers[1]); }
    }

    private void ActivateButt(bool on, SpriteRenderer buttSpriteRenderer)
    {
        if (on)
        {
            buttSpriteRenderer.sprite = leftHalfLaserOnSprite;
        }
        else
        {
            buttSpriteRenderer.sprite = leftHalfLaserOffSprite;
        }
    }

    private void ActivateHead(bool on, DinosaurLaser dinoLaser)
    {
        if (on)
        {
            dinoLaser.SetSprite(rightHalfLaserOnSprite);
            dinoLaser.EnableLaser(true);
        }
        else
        {
            dinoLaser.SetSprite(rightHalfLaserOffSprite);
            dinoLaser.EnableLaser(false);
        }
    }
    /*
    private IEnumerator ActivateHalvesInTime(bool on, float time)
    {
        SpriteRenderer leftHalf = connectedDinoButtSpriteRenderer;
        SpriteRenderer rightHalf = rightHalfSpriteRenderer;

        yield return new WaitForSeconds(time);

        if (on)
        {
            leftHalf.sprite = leftHalfLaserOnSprite;
            rightHalf.sprite = rightHalfLaserOnSprite;
            //laserGameObject.SetActive(true);
            laser.SetEnabled(true);
            laserOn = true;
        }
        else
        {
            leftHalf.sprite = leftHalfLaserOffSprite;
            rightHalf.sprite = rightHalfLaserOffSprite;
            //laserGameObject.SetActive(false);
            laser.SetEnabled(false);
            laserOn = false;
        }
    }*/

    public void FirstTimeActivate()
    {
        StartCoroutine(PlayFirstTimeActivationAnimation());
    }

    private IEnumerator PlayFirstTimeActivationAnimation()
    {
        //if (!(tileId == 'd')) { yield break; } //Only play this on the mirage laser

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

        foreach (DinosaurLaser dinoLaser in dinoLasers)
        {
            dinoLaser.EnableSpriteRenderer(true);//now using post laser dino head (broken skull)
        }

        //neverActivatedBefore = false;
        SaveSystem.Current.SetBool("desertDinoLaserActivatedAlready", true);

        SGridAnimator.OnSTileMoveEnd -= UpdateCanFirstTimeActivate;
        DesertArtifact.MirageDisappeared -= UpdateCanFirstTimeActivate;

        SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
        //SGridAnimator.OnSTileMoveStart += OnMoveStart;
        DesertArtifact.MirageDisappeared += OnMirageDisappeared;

        CheckEnableLasers();
        //StartCoroutine(ActivateHalvesInTime(true, 0f));
    }
}
