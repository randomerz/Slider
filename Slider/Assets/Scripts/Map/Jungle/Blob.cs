using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class Blob : MonoBehaviour
{

    public Animator animator; 

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

    public void UpdateBlobOnPath(bool defaultAnim, Direction direction, int travelDistance, Path pair, Shape shape)
    {
        carry = shape;
        SpriteRenderer spriteRenderer = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = carry.sprite;

        flip = defaultAnim;
        if (flip)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        this.direction = direction;
        this.travelDistance = travelDistance;
        this.pair = pair;
    }


    // Update is called once per frame
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
        //bro yes i should lerp this
        if (direction == Direction.RIGHT) {
            float target_X = this.transform.position.x + (speed * Time.deltaTime);
            float target_Y = jumpStart.y + -0.2f * time + 0.35f * (1 - (Mathf.Abs(0.5f - time) / 0.5f) * (Mathf.Abs(0.5f - time) / 0.5f));
            this.transform.position = new Vector3(target_X, target_Y);
        } else
        {
            float target_Y = jumpStart.y + 0.2f * time + 0.75f * (1 - (Mathf.Abs(0.5f - time) / 0.5f) * (Mathf.Abs(0.5f - time) / 0.5f));
            this.transform.position = new Vector3(this.transform.position.x, target_Y);
        }
    }

    //jump into bin
    public void JumpIntoBin()
    {
        //  GetComponent<Animation>().Play("BlobJumpRight");
        speed = 1f;
        jumping = true;
        jumpStart = new Vector2(this.transform.position.x, this.transform.position.y);
        StartCoroutine(WaitForJump());
    }

    IEnumerator WaitForJump()
    {
        animator.SetBool("Right", true);

        yield return new WaitForSeconds(jumpTime);
        Destroy(this.gameObject);
    }

    //fade in and fade out coroutines
}
