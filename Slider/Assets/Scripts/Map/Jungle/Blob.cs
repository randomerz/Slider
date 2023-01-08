using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class Blob : MonoBehaviour
{

    public Animator animator;
    public SpriteRenderer renderer;

    Direction direction;
    float travelDistance = 10;
    private float traveledDistance = 0;
    private Path pair;
    bool flip = false;
    float speed = 0.75f;
    bool jumping = false;
    float jumpTime = 1.75f;
    float timePassed = 0;
    Vector2 jumpStart;

    [Header ("shape")]
    public Shape carry;
    public SpriteRenderer shapeRenderer;

    public void UpdateBlobOnPath(bool defaultAnim, Direction direction, float travelDistance, Path pair, Shape shape)
    {
        carry = shape;
        SpriteRenderer spriteRenderer = shapeRenderer;
        spriteRenderer.sprite = carry.sprite;

        if (direction == Direction.LEFT || direction == Direction.RIGHT)
        {
            spriteRenderer.sortingOrder = -1;
            renderer.sortingOrder = -1;
        }

        flip = defaultAnim;
        if (flip)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        this.direction = direction;
        this.travelDistance = travelDistance;
        this.pair = pair;
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    public void setAlpha(float alpha)
    {
        Color c = renderer.material.color;
        c.a = alpha;
        Color s = shapeRenderer.material.color;
        s.a = alpha;
        renderer.material.color = c;
        shapeRenderer.material.color = s;
    }

    void FixedUpdate()
    {

        if (jumping)
        {
            Jump();
        } else
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
                renderer.sortingOrder = 0;
                shapeRenderer.sortingOrder = 0;
            }

            if (travelDistance - traveledDistance <= 1.25)
            {
                renderer.sortingOrder = -1;
                shapeRenderer.sortingOrder = -1;
            }

            // check if i need to change parent then if i do, change
            STile under = SGrid.GetStileUnderneath(this.gameObject);

            if (under == null)
            {
                Destroy(this.gameObject);
                return;
            }

            GameObject pathStile = this.transform.parent.transform.parent.transform.parent.gameObject;
            if (under.transform.gameObject != pathStile)
            {
                if (pair != null)
                {
                    this.gameObject.transform.SetParent(pair.transform);
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
        float time = timePassed / jumpTime; 
        if (direction == Direction.RIGHT) {
            float target_X = this.transform.position.x + (speed * Time.deltaTime);
            float target_Y = jumpStart.y + -2.5f * time + 1.5f * (1 - (Mathf.Abs(0.5f - time) / 0.5f) * (Mathf.Abs(0.5f - time) / 0.5f));
            this.transform.position = new Vector3(target_X, target_Y);
        } else
        {
            float target_Y = jumpStart.y + -1f * time + 1f * (1 - (Mathf.Abs(0.5f - time) / 0.5f) * (Mathf.Abs(0.5f - time) / 0.5f));
            this.transform.position = new Vector3(this.transform.position.x, target_Y);
        }
    }

    public void JumpIntoBin()
    {
        speed = 1.1f;
        jumping = true;
        jumpStart = new Vector2(this.transform.position.x, this.transform.position.y);
        StartCoroutine(WaitForJump());
    }

    IEnumerator WaitForJump()
    {
        animator.SetBool("Right", true);
        yield return new WaitForSeconds(jumpTime/2);
        renderer.sortingOrder = -1;
        shapeRenderer.sortingOrder = -1;
        yield return new WaitForSeconds(jumpTime / 2);
        Destroy(this.gameObject);
    }

    //fade in and fade out coroutines
    public IEnumerator fadeOutAnimation()
    {
        Color c = renderer.material.color;
        for (float alpha = 1f; alpha >= 0; alpha -= 0.1f)
        {
            c.a = alpha;
            renderer.material.color = c;
            shapeRenderer.material.color = c;
            yield return new WaitForSeconds(0.1667f);
        }
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
        Color c = renderer.material.color;
        for (float alpha = 0f; alpha <= 1; alpha += 0.25f)
        {
            c.a = alpha;
            renderer.material.color = c;
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
