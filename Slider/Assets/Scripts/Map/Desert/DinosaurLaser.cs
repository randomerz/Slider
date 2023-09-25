using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinosaurLaser : MonoBehaviour
{
    [SerializeField] private SpriteRenderer leftHalfSpriteRenderer;
    [SerializeField] private SpriteRenderer rightHalfSpriteRenderer;

    [SerializeField] private Sprite leftHalfLaserOffSprite;
    [SerializeField] private Sprite leftHalfLaserOnSprite;
    [SerializeField] private Sprite rightHalfLaserOffSprite;
    [SerializeField] private Sprite rightHalfLaserOnSprite;

    private void Start()
    {
        UIArtifact.MoveMadeOnArtifact += OnMoveEnd;
        //SGridAnimator.OnSTileMoveEnd += OnMoveEnd;
    }

    private void OnMoveEnd(object sender, System.EventArgs e)
    {
        Debug.Log(SGrid.GetGridString());
        if (CheckGrid.contains(SGrid.GetGridString(), "47"))
        {
            Debug.Log("LASER");
        }
    }
}
