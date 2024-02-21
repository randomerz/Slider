using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackalJump : MonoBehaviour
{
    private const string JUMP_SAVE_STRING = "desertJackalDidJump";
    //private const string TALKED_TO_JACKAL_SAVE_STRING = "DesertTalkedToJackal";
    private const string JACKAL_READY_FOR_JUMP_SAVE_STRING = "DesertJackalReadyForJump";
    // desertJackalOasis
    // desertJackalSeenBone

    public Transform jumpMidTransform;
    public Transform jumpEndTransform;
    public AnimationCurve jumpAnimationCurve;
    public AnimationCurve jumpHeightBonusCurve;
    private bool isJumping;
    public float jumpDuration = 0.5f;

    public GameObject skullGameObject;
    public GameObject jackalGameObject;
    public SpriteRenderer jackalSpriteRenderer;
    public Animator jackalAnimator;
    public Collider2D jackalCollider;

    private bool hasMiniBoost;
    private bool didJump;
    private bool finishedSkullWalk;

    private void OnEnable() 
    {
        SGridAnimator.OnSTileMoveEnd += OnSTileMove;
    }

    private void OnDisable() 
    {
        SGridAnimator.OnSTileMoveEnd -= OnSTileMove;
    }

    private void Start() 
    {
        if (SaveSystem.Current.GetBool(JUMP_SAVE_STRING))
        {
            if (!SaveSystem.Current.GetBool("desertJackalOasis"))
            {
                jackalGameObject.transform.position = jumpEndTransform.transform.position;
            }
            skullGameObject.SetActive(false);
            jackalAnimator.SetBool("hasSkull", true);
            didJump = true;
        }
    }
    
    
    private void OnSTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (SaveSystem.Current.GetBool(JUMP_SAVE_STRING) || !SaveSystem.Current.GetBool(JACKAL_READY_FOR_JUMP_SAVE_STRING))
            return;
        if (e.stile.islandId != 4)
            return;

        SMove smove = e.smove;
        Vector2Int d4 = Vector2Int.zero; // the difference tile 4's move makes
        foreach (Movement m in smove.moves)
        {
            if (m.islandId == 4)
            {
                d4 = m.endLoc - m.startLoc;
                break;
            }
        }
        
        if (d4 == new Vector2Int(-2, 0) && 
            e.prevPos == new Vector2Int(2, 2))
        {
            Jump();
        }
        else if (d4 == new Vector2Int(-1, 0) && 
            e.prevPos == new Vector2Int(1, 2))
        {
            hasMiniBoost = true;
        }
    }

    private void Jump()
    {
        SaveSystem.Current.SetBool(JUMP_SAVE_STRING, true);
        StartCoroutine(IJump());
    }

    private IEnumerator IJump()
    {
        jackalCollider.enabled = false;
        isJumping = true;
        jackalGameObject.transform.SetParent(jumpEndTransform);
        jackalSpriteRenderer.sortingOrder = 2;
        jackalAnimator.SetBool("isSpinning", true);
        jackalAnimator.SetBool("isFrozen", false);

        AudioManager.Play("Jackal Jump", jackalGameObject.transform);

        Vector3 startPos = jackalGameObject.transform.position;
        float t = 0;
        while (t < jumpDuration)
        {
            float x = jumpAnimationCurve.Evaluate(t / jumpDuration);
            Vector3 newPos = Vector3.Lerp(startPos, jumpMidTransform.position, x);
            newPos += Vector3.up * jumpHeightBonusCurve.Evaluate(t / jumpDuration);
            jackalGameObject.transform.position = newPos;

            yield return null;
            t += Time.deltaTime;
        }

        // skull emoji
        skullGameObject.SetActive(false);
        jackalAnimator.SetBool("hasSkull", true);
        
        t = 0;
        while (t < jumpDuration)
        {
            float x = jumpAnimationCurve.Evaluate(t / jumpDuration);
            Vector3 newPos = Vector3.Lerp(jumpMidTransform.position, jumpEndTransform.position, x);
            newPos += Vector3.up * jumpHeightBonusCurve.Evaluate(1 - (t / jumpDuration));
            jackalGameObject.transform.position = newPos;

            yield return null;
            t += Time.deltaTime;
        }

        jackalGameObject.transform.position = jumpEndTransform.position;

        jackalSpriteRenderer.sortingOrder = 0;
        jackalAnimator.SetBool("isSpinning", false);
        jackalCollider.enabled = true;
        isJumping = false;
        didJump = true;
    }

    public void SetFinishedSkullWalk(bool value)
    {
        finishedSkullWalk = value;
    }
    
    // NPC Conditionals
    public void HasMiniBoost(Condition c) => c.SetSpec(hasMiniBoost);
    public void IsJumping(Condition c)    => c.SetSpec(isJumping);
    public void DidJump(Condition c)      => c.SetSpec(didJump);
    public void FinishedSkullWalk(Condition c) => c.SetSpec(finishedSkullWalk);
}
