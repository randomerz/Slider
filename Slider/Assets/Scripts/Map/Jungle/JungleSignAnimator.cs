using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleSignAnimator : MonoBehaviour
{
    private int currentDirectionIndex = -1;
    private int currentShapeIndex = -1;
    private bool isGray;
    private Coroutine bumpCoroutine;
    private Sprite newSpriteBuffer;
    private bool finishedLateStart = false;

    [Header("Set references")]
    [SerializeField] private Sprite[] signDirectionsSprites; // right up left down
    [SerializeField] private Sprite[] signDirectionsSpritesGray; // right up left down
    [SerializeField] private Sprite[] bumpAnimationSprites; // bump1 bump2 -> normal sprite
    [SerializeField] private Sprite[] bumpAnimationSpritesGray; // bump1 bump2 -> normal sprite
    [SerializeField] private Sprite[] signShapeSprites; // triangle, circle, stick
    [SerializeField] private SpriteRenderer spriteRenderer;


    private const float ANIMATION_DELAY = 0.06125f;

    private void Start()
    {
        StartCoroutine(LaterStart());
    }

    // Skip audio if sign was set in LateStart() by JungleBox
    private IEnumerator LaterStart()
    {
        yield return null;

        finishedLateStart = true;
    }

    private void OnDisable()
    {
        if (bumpCoroutine != null)
        {
            FinishBumpAnimation();
        }
    }

    /// <summary>
    /// Set's the sign sprites direction with a small sprite animation.
    /// </summary>
    /// <param name="direction">Must be unit vector in the cardinal direction</param>
    public void SetDirection(Vector2 direction)
    {
        int index = DirectionToSpriteIndex(direction);
        if (index == currentDirectionIndex)
            return;
            
        if (bumpCoroutine != null)
            StopCoroutine(bumpCoroutine);
        
        currentDirectionIndex = index;
        Sprite newSprite = isGray ? signDirectionsSpritesGray[index] : 
                                    signDirectionsSprites[index];

        bumpCoroutine = StartCoroutine(BumpAnimation(newSprite));
    }

    // Only for direction arrows
    public void SetIsGray(bool value)
    {
        if (isGray != value)
        {
            isGray = value;

            if (currentDirectionIndex == -1)
            {
                if (finishedLateStart)
                {
                    Debug.LogWarning("Couldn't find index of jungle sign sprite.");
                }
                return;
            }

            Sprite newSprite = isGray ? signDirectionsSpritesGray[currentDirectionIndex] : 
                                        signDirectionsSprites[currentDirectionIndex];

            newSpriteBuffer = newSprite;
            if (bumpCoroutine == null)
            {
                spriteRenderer.sprite = newSprite;
                // ParticleManager.SpawnParticle(ParticleType.MiniSparkle, transform.position, transform);
            }
        }
    }

    public void SetShapeIndex(int index)
    {       
        if (currentShapeIndex == index)
            return;
            
        if (bumpCoroutine != null)
            StopCoroutine(bumpCoroutine);

        currentShapeIndex = index;
        bumpCoroutine = StartCoroutine(BumpAnimation(signShapeSprites[index]));
    }

    private IEnumerator BumpAnimation(Sprite newSprite)
    {
        newSpriteBuffer = newSprite;
        spriteRenderer.sprite = isGray ? bumpAnimationSpritesGray[0] :
                                         bumpAnimationSprites[0];

        if (finishedLateStart)
        {
            AudioManager.Play("UI Click");

            yield return new WaitForSeconds(ANIMATION_DELAY);

            spriteRenderer.sprite = isGray ? bumpAnimationSpritesGray[1] :
                                            bumpAnimationSprites[1];

            yield return new WaitForSeconds(ANIMATION_DELAY);
        }

        FinishBumpAnimation();
    }

    private void FinishBumpAnimation()
    {
        spriteRenderer.sprite = newSpriteBuffer;
        bumpCoroutine = null;
    }
    
    private int DirectionToSpriteIndex(Vector2 direction)
    {
        return (int)(Mathf.Atan2(direction.y, direction.x) / (Mathf.PI / 2) + 4) % 4;
    }

    // for debug
    public void SetRandomDirection()
    {
        SetDirection(new Vector2[] {Vector2.right, Vector2.up, Vector2.left, Vector2.down}[Random.Range(0, 4)]);
    }

}
