using System.Collections;
using UnityEngine;
//using static Cinemachine.CinemachineSmoothPath;

public class DinoLasersManager : MonoBehaviour
{
    [SerializeField] private bool debugSkipFezziwigActivation = false; //allows lasers on from the start

    [SerializeField] private Sprite leftHalfLaserOffSprite;
    [SerializeField] private Sprite leftHalfLaserOnSprite;
    [SerializeField] private Sprite rightHalfLaserOffSprite;
    [SerializeField] private Sprite rightHalfLaserOnSprite;
    
    [SerializeField] private DinoLaser[] dinoLasers; //0 is normal tile 4 laser, 1 is mirage tile 4 laser 
    [SerializeField] private SpriteRenderer[] dinoButtSpriteRenderers; //0 is normal tile 7 butt, 1 is mirage tile 7 butt 

    private bool moveEndWasCheckedThisFrame = false;
    //private bool moveStartWasCheckedThisFrame = false;

    [SerializeField] private GameObject firstTimeActivationAnimation;
    private const string desertDinoLaserActivatedAlready = "desertDinoLaserActivatedAlready";

    private bool canFirstTimeActivate = false;
    public void CheckCanFirstTimeActivate(Condition c) { c.SetSpec(canFirstTimeActivate); }

    private void Start()
    {
       SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (debugSkipFezziwigActivation)
        {
            Destroy(firstTimeActivationAnimation);

            foreach (DinoLaser dinoLaser in dinoLasers)
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
        //moveStartWasCheckedThisFrame = false;
    }

    private void OnDisable()
    {
        UnSubscribeFromEvents();
        RemoveAllLasersPermanently();
    }

    private void UnSubscribeFromEvents()
    {
        if (debugSkipFezziwigActivation)
        {
            SGridAnimator.OnSTileMoveEnd -= OnMoveEnd;
            DesertArtifact.MirageDisappeared -= OnMirageDisappeared;
        }
        else
        {
            SGridAnimator.OnSTileMoveEnd -= UpdateCanFirstTimeActivate;
            DesertArtifact.MirageDisappeared -= UpdateCanFirstTimeActivate;
        }
    }

    private void UpdateCanFirstTimeActivate(object sender, System.EventArgs e)
    {
        bool activatedPreviously = SaveSystem.Current.GetBool(desertDinoLaserActivatedAlready);

        string gridString = DesertGrid.GetGridString();
        //Debug.Log("FIRST TIME ACTIVATE LASER Checking " + gridString);

        if (!activatedPreviously && (CheckGrid.contains(gridString, "7D") || CheckGrid.contains(gridString, "GD"))) //normal tail | mirage head
        {
            canFirstTimeActivate = true;
        }
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
    /*
    private void OnMoveStart(object sender, System.EventArgs e)
    {
        if (!moveStartWasCheckedThisFrame)
        {
            moveStartWasCheckedThisFrame = true;
            CheckDisableLasers();
        }
    }*/

    private void OnMirageDisappeared(object sender, System.EventArgs e)
    {
        //Debug.Log("On Mirage Disaapear");
        CheckDisableLasers();
    }

    private void CheckEnableLasers()
    {
        string gridString = DesertGrid.GetGridString();
        //Debug.Log("ENABLE LASERS Checking " + gridString);

        if (CheckGrid.contains(gridString, "74")) //normal tail | normal head
        {
            ActivateButt(true, dinoButtSpriteRenderers[0]);
            ActivateHead(true, dinoLasers[0]);
        }

        if (CheckGrid.contains(gridString, "G4")) //mirage tail | normal head
        {
            ActivateButt(true, dinoButtSpriteRenderers[1]);
            ActivateHead(true, dinoLasers[0]);
        }

        if (CheckGrid.contains(gridString, "7D")) //normal tail | mirage head
        {
            ActivateButt(true, dinoButtSpriteRenderers[0]);
            ActivateHead(true, dinoLasers[1]);
        }

        if (CheckGrid.contains(gridString, "GD")) //mirage tail | mirage head
        {
            ActivateButt(true, dinoButtSpriteRenderers[1]);
            ActivateHead(true, dinoLasers[1]);
        }
    }

    private void CheckDisableLasers()
    {
        string gridString = DesertGrid.GetGridString();
        //Debug.Log("DISABLE LASERS Checking " + gridString);

        bool normalButtConnected = false;
        bool normalHeadConnected = false;
        bool mirageButtConnected = false;
        bool mirageHeadConnected = false;

        if (CheckGrid.contains(gridString, "74")) //normal tail | normal head
        {
            normalButtConnected = true;
            normalHeadConnected = true;
        }

        if (CheckGrid.contains(gridString, "G4")) //mirage tail | normal head
        {
            mirageButtConnected = true;
            normalHeadConnected = true;
        }

        if (CheckGrid.contains(gridString, "7D")) //normal tail | mirage head
        {
            normalButtConnected = true;
            mirageHeadConnected = true;
        }

        if (CheckGrid.contains(gridString, "GD")) //mirage tail | mirage head
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

    private void ActivateHead(bool on, DinoLaser dinoLaser)
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

    public void FirstTimeActivate()
    {
        StartCoroutine(PlayFirstTimeActivationAnimation());
    }

    private IEnumerator PlayFirstTimeActivationAnimation()
    {
        yield return new WaitForSeconds(2);

        firstTimeActivationAnimation.SetActive(true);

        yield return new WaitForSeconds(2.167f); // We love magic waits - time of the animation

        firstTimeActivationAnimation.SetActive(false);
        //Destroy(firstTimeActivationAnimation);

        foreach (DinoLaser dinoLaser in dinoLasers)
        {
            dinoLaser.EnableSpriteRenderer(true); // now using post laser dino head (broken skull)
        }

        SaveSystem.Current.SetBool("desertDinoLaserActivatedAlready", true);

        SGridAnimator.OnSTileMoveEnd -= UpdateCanFirstTimeActivate;
        DesertArtifact.MirageDisappeared -= UpdateCanFirstTimeActivate;

        SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
        DesertArtifact.MirageDisappeared += OnMirageDisappeared;

        CheckEnableLasers();
    }

    public void RemoveAllLasersPermanently()
    {
        foreach (DinoLaser dinoLaser in dinoLasers)
        {
            ActivateHead(false, dinoLaser);
        }

        foreach (SpriteRenderer dinoButt in dinoButtSpriteRenderers)
        {
            ActivateButt(false, dinoButt);
        }

        SGridAnimator.OnSTileMoveEnd -= OnMoveEnd;
        //SGridAnimator.OnSTileMoveStart -= OnMoveStart;
        DesertArtifact.MirageDisappeared -= OnMirageDisappeared;
    }
}
