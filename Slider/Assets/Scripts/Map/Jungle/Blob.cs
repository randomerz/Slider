using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{

    public Animator animator; 

    Direction direction;
    float travelDistance = 10;
    private float traveledDistance = 0;
    private Path pair;
    bool flip = false;

    float speed = 1f;
    public Shape carry;

    public void UpdateBlobOnPath(bool defaultAnim, Direction direction, float blobspeed, int travelDistance, Path pair, Shape shape)
    {
        carry = shape;
        this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = carry.sprite;

        flip = defaultAnim;
        if (flip)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        this.direction = direction;
        this.speed = blobspeed;
        this.travelDistance = travelDistance;
        this.pair = pair;
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 new_distance = DirectionUtil.D2V(direction) * (speed * 0.01f);
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

    //fade in and fade out coroutines
}
