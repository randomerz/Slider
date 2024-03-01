using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleSignAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private Vector2 direction;
    private Coroutine bumpCoroutine;

    [Header("Set references")]
    public Sprite[] signDirectionsSprites; // right up left down
    public Sprite[] bumpAnimationSprites; // bump1 bump2 -> normal sprite
    public Sprite[] signShapeSprites; //triangle, circle, stick
    [SerializeField] private Sign sign;
    [SerializeField] private Hut hut;


    private const float ANIMATION_DELAY = 0.06125f;

    /// <summary>
    /// Set's the sign sprites direction with a small sprite animation.
    /// </summary>
    /// <param name="direction">Must be unit vector in the cardinal direction</param>
    public void SetDirection(Vector2 direction)
    {
        if (this.direction == direction)
            return;
            
        if (bumpCoroutine != null)
            StopCoroutine(bumpCoroutine);
        
        this.direction = direction;
        bumpCoroutine = StartCoroutine(BumpAnimation(direction));
    }

    private IEnumerator BumpAnimation(Vector2 direction)
    {
        spriteRenderer.sprite = bumpAnimationSprites[0];
        AudioManager.Play("UI Click");

        yield return new WaitForSeconds(ANIMATION_DELAY);

        spriteRenderer.sprite = bumpAnimationSprites[1];

        yield return new WaitForSeconds(ANIMATION_DELAY);

        spriteRenderer.sprite = DirectionToSprite(direction);
        bumpCoroutine = null;
    }
    private IEnumerator BumpAnimation(int shapeIndex)
    {
        spriteRenderer.sprite = bumpAnimationSprites[0];
        AudioManager.Play("UI Click");

        yield return new WaitForSeconds(ANIMATION_DELAY);

        spriteRenderer.sprite = bumpAnimationSprites[1];

        yield return new WaitForSeconds(ANIMATION_DELAY);

        spriteRenderer.sprite = signShapeSprites[shapeIndex];
        bumpCoroutine = null;
    }
    private Sprite DirectionToSprite(Vector2 direction)
    {
        int index = (int)(Mathf.Atan2(direction.y, direction.x) / (Mathf.PI / 2) + 4) % 4;
        // Debug.Log(index + " " + Mathf.Atan2(direction.y, direction.x));
       // print(direction + " : " + index);
        return signDirectionsSprites[index];
    }

    // for debug
    public void SetRandomDirection()
    {
        SetDirection(new Vector2[] {Vector2.right, Vector2.up, Vector2.left, Vector2.down}[Random.Range(0, 4)]);
    }

    public void UpdateDirection()
    {
        if (hut != null)
        {
            Vector2 direction = hut.GetDirection();
           // print(direction);
            SetDirection(direction);
        } else if (sign != null)
        {
            Vector2 direction = sign.GetDirection();
            SetDirection(direction);
        } else
        {
            SetRandomDirection();
        }
    }

    public void UpdateShape()
    {       
        if (hut != null)
        {
            if (bumpCoroutine != null)
                StopCoroutine(bumpCoroutine);
            bumpCoroutine = StartCoroutine(BumpAnimation(hut.currentShapeIndex));
        }
    }
}
