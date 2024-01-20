using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class Blob : MonoBehaviour
{

    public Animator animator;
    public SpriteRenderer spriteRenderer;

    Direction direction;
    float travelDistance = 10;
    public float traveledDistance = 0;
    private Path pair;
    bool flip = false;
    float speed = 0.75f;
    bool jumping = false;
    float jumpTime = 1.4f;
    float timePassed = 0;

    [Header("shape")]
    public Shape carry;
    public SpriteRenderer shapeRenderer;

    [Header("Jump info")]
    private Vector2 startPos;
    private Vector2 targetPos;

    public void UpdateBlobOnPath(bool defaultAnim, Direction direction, float travelDistance, Path pair, Shape shape)
    {
        carry = shape;
        SpriteRenderer spriteRenderer = shapeRenderer;
        spriteRenderer.sprite = carry.fullSprite;

        flip = defaultAnim;
        if (flip)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        this.direction = direction;
        this.travelDistance = travelDistance;
        this.pair = pair;

        /*        if ((direction == Direction.LEFT || direction == Direction.RIGHT))
                {
                    renderer.sortingOrder = -2;
                    shapeRenderer.sortingOrder = -2;
                }*/
    }

    public void setTraveledDistance(float traveled)
    {
        this.traveledDistance = traveled;
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    public void setAlpha(float alpha)
    {
        Color c = spriteRenderer.material.color;
        c.a = alpha;
        Color s = shapeRenderer.material.color;
        s.a = alpha;
        spriteRenderer.material.color = c;
        shapeRenderer.material.color = s;
    }

    void FixedUpdate()
    {
        if (jumping)
        {
            Jump();
        }
        else
        {
            Vector2 new_distance = DirectionUtil.D2V(direction) * (speed * Time.deltaTime);
            traveledDistance += Mathf.Abs(new_distance.magnitude);
            this.transform.position = this.transform.position + new Vector3(new_distance.x, new_distance.y, 0);

            if (traveledDistance >= travelDistance)
            {
                Destroy(this.gameObject);
            }

            if (traveledDistance > 2 && traveledDistance < 4)
            {
                spriteRenderer.sortingOrder = 0;
                shapeRenderer.sortingOrder = 0;
            }

            // check if i need to change parent then if i do, change
            STile under = SGrid.GetSTileUnderneath(this.gameObject);

            if (under == null)
            {
                //Destroy(this.gameObject);
                return;
            }

            GameObject path = this.transform.parent.gameObject;
            GameObject pathStile = path.transform.parent.transform.parent.transform.parent.gameObject; //bro pls theres a better way right
            if (under.transform.gameObject != pathStile)
            {
                if (pair != null)
                {
                    this.transform.SetParent(pair.transform);
                    Vector2 position = this.transform.localPosition;
                    position.y = 0;
                    this.gameObject.transform.localPosition = position;
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
        }

    }

    void Jump()
    {
        timePassed += Time.deltaTime;
        if (direction == Direction.RIGHT)
        {
            float dist = targetPos.x - startPos.x;
            float nextX = Mathf.MoveTowards(transform.position.x, targetPos.x, speed * Time.deltaTime);
            float baseY = Mathf.Lerp(startPos.y, targetPos.y, (nextX - startPos.x) / dist);
            float height = 1.5f * (nextX - startPos.x) * (nextX - targetPos.x) / (-0.25f * dist * dist);

            Vector3 movePosition = new Vector3(nextX, baseY + height, transform.position.z);
            transform.position = movePosition;
        }
        else
        {
            float time = timePassed / jumpTime;
            float target_Y = startPos.y + -1f * time + 1f * (1 - (Mathf.Abs(0.5f - time) / 0.5f) * (Mathf.Abs(0.5f - time) / 0.5f));
            this.transform.position = new Vector3(this.transform.position.x, target_Y);
        }
    }

    public void JumpIntoBin()
    {
        speed = 1.2f;
        jumping = true;
        if (direction == Direction.RIGHT)
        {
            jumpTime = 2.5f;
        }
        StartCoroutine(WaitForJump());
        startPos = new Vector2(this.transform.position.x, this.transform.position.y);
        targetPos = new Vector2(this.transform.position.x + 3, this.transform.position.y - 3);
    }

    IEnumerator WaitForJump()
    {
        animator.SetBool("Right", true);
        yield return new WaitForSeconds(jumpTime / 2);
        spriteRenderer.sortingOrder = -2;
        shapeRenderer.sortingOrder = -2;
        yield return new WaitForSeconds(jumpTime / 2);
        Destroy(this.gameObject);
    }

    //fade in and fade out coroutines
    public IEnumerator fadeOutAnimation()
    {
        Color c = spriteRenderer.material.color;

        for (float alpha = c.a; alpha >= 0; alpha -= 0.25f)
        {
            if (alpha < 0)
            {
                alpha = 0;
            }

            c.a = alpha;
            spriteRenderer.material.color = c;
            shapeRenderer.material.color = c;
            yield return new WaitForSeconds(0.1667f);
        }
        Destroy(this.gameObject);
    }

    public void fadeOut()
    {
        if (!jumping)
        {
            StartCoroutine(fadeOutAnimation());
            speed = 0;
        }
    }

    public IEnumerator fadeInAnimation()
    {
        Color c = spriteRenderer.material.color;
        for (float alpha = 0f; alpha <= 1 && speed > 0; alpha += 0.25f)
        {
            c.a = alpha;
            spriteRenderer.material.color = c;
            shapeRenderer.material.color = c;
            yield return new WaitForSeconds(0.1667f);
        }
    }

    public void fadeIn()
    {
        if (!jumping)
        {
            speed = 0.75f;
            StartCoroutine(fadeInAnimation());
        }
    }
}
