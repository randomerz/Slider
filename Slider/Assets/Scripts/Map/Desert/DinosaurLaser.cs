using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineSmoothPath;

public class DinosaurLaser : MonoBehaviour
{
    [SerializeField] private CameraDolly cameraDolly;
    [SerializeField] private GameObject firstTimeActivationAnimation;
    [SerializeField] private List<SpriteRenderer> leftHalfSpriteRenderers;
    [SerializeField] private SpriteRenderer rightHalfSpriteRenderer;

    [SerializeField] private Sprite leftHalfLaserOffSprite;
    [SerializeField] private Sprite leftHalfLaserOnSprite;
    [SerializeField] private Sprite rightHalfLaserOffSprite;
    [SerializeField] private Sprite rightHalfLaserOnSprite;

    [SerializeField] private bool laserOn = false;
    [SerializeField] private /*static*/bool neverActivatedBefore = true;

    private GameObject laserGameObject;

    private void Start()
    {
        laserGameObject = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
        CheckEnableOrDisableLaser();
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveEnd -= OnMoveEnd;
    }

    private void OnMoveEnd(object sender, System.EventArgs e)
    {
        const float timeBetweenChecksAllowed = 0.1f;
        if (Time.time >  lastTimeChecked + timeBetweenChecksAllowed) //someone kill me
        {
            CheckEnableOrDisableLaser();
        }
    }

    private SpriteRenderer currentDinoButt;
    private float lastTimeChecked = 0;

    private void CheckEnableOrDisableLaser()
    {

                /*
        string gridString = SGrid.GetGridString();
        Debug.Log(gridString);
        if (CheckGrid.contains(gridString, "47"))
        {
            Debug.Log("LASER");
        }
        else if (CheckGrid.contains(gridString, "#7") || CheckGrid.contains(gridString, "4#"))
        {

        }*/
        SpriteRenderer dinoButt = GetDinoButtOnTheLeft();
        if (dinoButt == null && laserOn) //no dino butt on the left
        {
            Debug.Log("turn off");
            //dinoButt.sprite = leftHalfLaserOffSprite;
            //rightHalfSpriteRenderer.sprite = rightHalfLaserOffSprite;
            StartCoroutine(ActivateHalvesInTime(false, 0.01f, currentDinoButt, rightHalfSpriteRenderer));
            currentDinoButt = null;
            laserOn = false;
        } 
        else if (dinoButt != null && !laserOn)
        {
            Debug.Log("turn on");

            StartCoroutine(ActivateHalvesInTime(true, 0.5f, dinoButt, rightHalfSpriteRenderer));
            currentDinoButt = dinoButt;
            laserOn = true;
        }
        lastTimeChecked= Time.time;
    }

    private IEnumerator ActivateHalvesInTime(bool on, float time, SpriteRenderer leftHalf, SpriteRenderer rightHalf)
    {
        if(neverActivatedBefore && on && firstTimeActivationAnimation != null)
        {
            yield return PlayFirstTimeActivationAnimation();
        }
        else
        {
            yield return new WaitForSeconds(time);
        }
        if (on)
        {
            leftHalf.sprite = leftHalfLaserOnSprite;
            rightHalf.sprite = rightHalfLaserOnSprite;
            laserGameObject.SetActive(true);
        }
        else
        {
            leftHalf.sprite = leftHalfLaserOffSprite;
            rightHalf.sprite = rightHalfLaserOffSprite;
            laserGameObject.SetActive(false);
        }
    }

    private IEnumerator PlayFirstTimeActivationAnimation()
    {
        Debug.Log("first time animation");
        //cameraDolly.path.m_Waypoints;
        Waypoint waypoint = new Waypoint();
        waypoint.position = transform.position - new Vector3(5, 0, 0);
        Waypoint[] waypoints = { waypoint };
        cameraDolly.path.m_Waypoints = waypoints;

        cameraDolly.StartTrack();
        yield return new WaitForSeconds(2);

        firstTimeActivationAnimation.SetActive(true);
        //firstTimeActivationAnimation.GetComponent<Animator>().Play();
        yield return new WaitForSeconds(2.167f);

        firstTimeActivationAnimation.SetActive(false);
        Destroy(firstTimeActivationAnimation);
        neverActivatedBefore = false;
    }

    private SpriteRenderer GetDinoButtOnTheLeft()
    {
        const float distMinX = 4;
        const float distMaxX = 15;
        const float distMaxY = 4;
        Vector2 rightHalfPos = transform.position;

        foreach (SpriteRenderer leftHalfSpriteRenderer in leftHalfSpriteRenderers)
        {
            if (!leftHalfSpriteRenderer.gameObject.activeInHierarchy) { continue; }

            Vector2 leftHalfPos = leftHalfSpriteRenderer.transform.position;
            float diffX = rightHalfPos.x - leftHalfPos.x;
            float diffY = Mathf.Abs( rightHalfPos.y - leftHalfPos.y);
            if (diffX > distMinX && diffX < distMaxX && diffY < distMaxY)
            {
                return leftHalfSpriteRenderer;
            }
        }
            return null;
    }
}
