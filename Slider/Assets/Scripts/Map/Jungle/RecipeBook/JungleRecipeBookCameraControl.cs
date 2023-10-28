using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class JungleRecipeBookCameraControl : MonoBehaviour
{
    public CinemachineVirtualCamera recipeBookCamera;

    private const int LOOKING_AT_TV_PRIORITY = 15;
    private const int NOT_LOOKING_AT_TV_PRIORITY = 0;

    public void SetLookingAtTV(bool value)
    {
        recipeBookCamera.Priority = value ? LOOKING_AT_TV_PRIORITY : NOT_LOOKING_AT_TV_PRIORITY;
    }
}
