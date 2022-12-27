using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TrailerCutScene : MonoBehaviour
{
    //public CinemachineVirtualCamera cmPlayerCam;
    //public CinemachineVirtualCamera cmDollyCam;

    //public GameObject catGO;
    //public GameObject catFootPrintsGO;
    //public Collider2D npcDialogueCollider;

    //public float lookAtCatWait = 0.5f;
    //public float dollyWait = 1f;

    public Animator catAnimator;
    public Animator oldManAnimator;
    public SpriteRenderer oldManSpriteRenderer;

    public GameObject slider3;
    public GameObject tile3;
    public GameObject tile5;
    public GameObject tile7;
    public GameObject tile9;

    public AnimationCurve shrinkCurve;
    public float shrinkDuration;

    void Start()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(StartIntroCutscene());
    }

    //private IEnumerator StartIntroCutscene()
    //{
    //    Debug.Log("Looking at cat...");
    //    yield return new WaitForSeconds(lookAtCatWait);

    //    Debug.Log("Updating Dolly path...");
    //    cmDollyCam.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = 1;

    //    yield return new WaitForSeconds(dollyWait);

    //    Debug.Log("Tracking back to Player...");
    //    cmDollyCam.Priority = -1;

    //    catGO.SetActive(false);
    //    catFootPrintsGO.SetActive(true);
    //    npcDialogueCollider.enabled = true;
    //}

    private IEnumerator StartIntroCutscene()
    {
        Debug.Log("Starting...");

        // camera shake
        CameraShake.Shake(1, 0.5f);

        yield return new WaitForSeconds(0.25f);

        // slow zoom
        StartCoroutine(SlowCameraZoom(2, 5));

        // cat awake
        catAnimator.SetBool("awake", true);

        yield return new WaitForSeconds(0.25f);

        // old man sus
        oldManAnimator.SetBool("sus", true);

        yield return new WaitForSeconds(0.5f);

        // old man turn around
        oldManSpriteRenderer.flipX = !oldManSpriteRenderer.flipX;

        yield return new WaitForSeconds(0.25f);

        // shrink tile
        // poof + spawn slider

        StartCoroutine(ShrinkTile(tile3, shrinkDuration));

        // shrink other tiles

        yield return new WaitForSeconds(2);

        StartCoroutine(ShrinkTile(tile5, shrinkDuration));
        yield return new WaitForSeconds(0.125f);
        StartCoroutine(ShrinkTile(tile7, shrinkDuration));
        yield return new WaitForSeconds(0.125f);
        StartCoroutine(ShrinkTile(tile9, shrinkDuration));
    }

    private IEnumerator ShrinkTile(GameObject slider, float duration)
    {
        Debug.Log("Starting camera zoom");

        float t = 0;
        while (t < duration)
        {
            float scale = 1;
            slider.transform.localScale = scale * Vector3.one;

            yield return null;
            t += Time.deltaTime;
        }

        Debug.Log("Finished zoom");

        GameObject smokePrefab = ParticleManager.GetPrefab(ParticleType.SmokePoof);

        yield return null;

        slider3.SetActive(true);
    }

    private IEnumerator SlowCameraZoom(float duration, float endZoomAmount)
    {
        Debug.Log("Starting camera zoom");

        float t = 0;
        while (t < duration)
        {
            // set zoom

            yield return null;
            t += Time.deltaTime;
        }
    }
}
