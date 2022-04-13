using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TrailerCutScene : MonoBehaviour
{
    public CinemachineVirtualCamera cmPlayerCam;
    public CinemachineVirtualCamera cmDollyCam;

    public GameObject catGO;
    public GameObject catFootPrintsGO;
    public Collider2D npcDialogueCollider;

    public float lookAtCatWait = 0.5f;
    public float dollyWait = 1f;

    void Start()
    {
        StartCoroutine(StartIntroCutscene());
    }

    private IEnumerator StartIntroCutscene()
    {
        Debug.Log("Looking at cat...");
        yield return new WaitForSeconds(lookAtCatWait);

        Debug.Log("Updating Dolly path...");
        cmDollyCam.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = 1;

        yield return new WaitForSeconds(dollyWait);

        Debug.Log("Tracking back to Player...");
        cmDollyCam.Priority = -1;

        catGO.SetActive(false);
        catFootPrintsGO.SetActive(true);
        npcDialogueCollider.enabled = true;
    }
}
