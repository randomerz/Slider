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

    public CinemachineVirtualCamera cmPlayerCam;
    public AnimationCurve cameraZoomCurve;

    public Animator catAnimator;
    public Animator oldManAnimator;
    public SpriteRenderer oldManSpriteRenderer;

    public GameObject slider3;
    public GameObject tile3;
    public GameObject tile5;
    public GameObject tile7;
    public GameObject tile9;

    public AnimationCurve shrinkCurve;
    public float shrinkEndRotation;
    public float shrinkDuration;

    void Start()
    {
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
        yield return new WaitForSeconds(3);

        Debug.Log("Starting...");

        // camera shake
        CameraShake.Shake(1, 0.5f);

        // slow zoom
        StartCoroutine(SlowCameraZoom(4, 7));

        yield return new WaitForSeconds(0.25f);


        // cat awake
        catAnimator.SetBool("isAwake", true);

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

        yield return new WaitForSeconds(0.5f);
        oldManSpriteRenderer.flipX = !oldManSpriteRenderer.flipX;

        // shrink other tiles

        yield return new WaitForSeconds(1f);

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
        float x = 0;
        while (t < duration)
        {
            x = shrinkCurve.Evaluate(t / duration);
            slider.transform.rotation = Quaternion.Euler(0, 0, x * shrinkEndRotation);
            slider.transform.localScale = (1 - x) * Vector3.one;

            yield return null;
            t += Time.deltaTime;
        }

        Debug.Log("Finished zoom");
        
        slider.transform.localEulerAngles.Set(0, 0, shrinkEndRotation);
        slider.transform.localScale = Vector3.zero;

        GameObject smokePrefab = ParticleManager.GetPrefab(ParticleType.SmokePoof);
        Instantiate(smokePrefab, slider.transform.position, Quaternion.identity, null);
        Instantiate(smokePrefab, slider.transform.position, Quaternion.identity, null);

        yield return null;

        slider3.SetActive(true);
    }

    private IEnumerator SlowCameraZoom(float duration, float endZoomAmount)
    {
        Debug.Log("Starting camera zoom");

        float startZoomAmount = cmPlayerCam.m_Lens.OrthographicSize;

        float t = 0;
        while (t < duration)
        {
            // set zoom
            float x = cameraZoomCurve.Evaluate(t / duration);
            float newZoom = Mathf.Lerp(startZoomAmount, endZoomAmount, x);
            cmPlayerCam.m_Lens.OrthographicSize = newZoom;

            yield return null;
            t += Time.deltaTime;
        }

        cmPlayerCam.m_Lens.OrthographicSize = endZoomAmount;
    }
}
