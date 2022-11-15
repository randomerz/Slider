using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{

    public Animator animator; 

    public Direction direction;
    public float travelDistance = 10;
    private float traveledDistance = 0;
    bool flip = false;

    public float speed = 1f;

    public void UpdateBlobOnPath(bool defaultAnim, Direction direction, float blobspeed, int travelDistance)
    {
        flip = defaultAnim;
        if (flip)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        this.direction = direction;
        this.speed = blobspeed;
        this.travelDistance = travelDistance;
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
    }
}
